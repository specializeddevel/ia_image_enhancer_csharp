using System.Collections.Concurrent;
using ImageProcessor.Core;

namespace ImageProcessor.Api;

public class JobService
{
    private readonly ImageProcessorService _processorService;
    private readonly ILogger<JobService> _logger;
    private readonly ProcessingLogService _logService;
    private readonly ConcurrentDictionary<string, Job> _jobs = new();

    public JobService(ImageProcessorService processorService, ILogger<JobService> logger, ProcessingLogService logService)
    {
        _processorService = processorService;
        _logger = logger;
        _logService = logService;
    }

    public Job CreateJob(ProcessingOptions options)
    {
        var job = new Job { Options = options };
        _jobs[job.Id] = job;
        return job;
    }

    public Job? GetJob(string id)
    {
        _jobs.TryGetValue(id, out var job);
        return job;
    }

    public void StartJob(Job job)
    {
        job.Status = JobStatus.Running;

        _ = Task.Run(async () =>
        {
            var progress = new Progress<ProcessingUpdate>(update =>
            {
                job.ProgressHistory.Add(update);
                if (update.IsError)
                {
                    job.Status = JobStatus.Failed;
                }
                else if (update.IsComplete)
                {
                    job.Status = JobStatus.Completed;
                }
            });

            try
            {
                if (job.Options is null)
                {
                    throw new InvalidOperationException("Job options are not set.");
                }

                var logEntries = await _processorService.ProcessImagesAsync(job.Options, progress, CancellationToken.None);
                foreach (var entry in logEntries)
                {
                    _logService.Log(entry);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during background processing for job {JobId}.", job.Id);
                job.Status = JobStatus.Failed;
                job.ProgressHistory.Add(new ProcessingUpdate { Message = ex.Message, IsError = true });
            }
        });
    }
}

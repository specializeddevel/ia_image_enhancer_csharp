using ImageProcessor.Core;

namespace ImageProcessor.Api;

public class Job
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
        public ProcessingOptions? Options { get; set; }
    public JobStatus Status { get; set; } = JobStatus.Pending;
    public List<ProcessingUpdate> ProgressHistory { get; set; } = new();
    public ProcessingUpdate? LastUpdate => ProgressHistory.LastOrDefault();
}

public enum JobStatus
{
    Pending,
    Running,
    Completed,
    Failed,
    Canceled
}

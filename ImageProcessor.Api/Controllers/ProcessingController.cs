
using ImageProcessor.Core;
using Microsoft.AspNetCore.Mvc;

namespace ImageProcessor.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProcessingController : ControllerBase
{
    private readonly JobService _jobService;

    public ProcessingController(JobService jobService)
    {
        _jobService = jobService;
    }

    [HttpPost("start")]
    public IActionResult StartProcessing([FromBody] ProcessingOptions options)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var job = _jobService.CreateJob(options);
        _jobService.StartJob(job);

        return Accepted(new { jobId = job.Id });
    }

    [HttpGet("{jobId}/status")]
    public IActionResult GetJobStatus(string jobId)
    {
        var job = _jobService.GetJob(jobId);
        if (job == null)
        {
            return NotFound();
        }

        return Ok(new
        {
            job.Id,
            job.Status,
            LastUpdate = job.LastUpdate
        });
    }

    [HttpGet("{jobId}/history")]
    public IActionResult GetJobHistory(string jobId)
    {
        var job = _jobService.GetJob(jobId);
        if (job == null)
        {
            return NotFound();
        }

        return Ok(job.ProgressHistory);
    }
}

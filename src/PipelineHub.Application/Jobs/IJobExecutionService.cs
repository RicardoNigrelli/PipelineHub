namespace PipelineHub.Application.Jobs;

/// <summary>
/// The actual "do the job" step, invoked by the background queue (Hangfire) rather than
/// inline in the HTTP request. Public so Hangfire's DI-based activator can resolve and call it.
/// </summary>
public interface IJobExecutionService
{
    Task ExecuteAsync(Guid jobId, CancellationToken cancellationToken);
}

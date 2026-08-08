using Microsoft.Extensions.Logging;
using PipelineHub.Domain;

namespace PipelineHub.Application.Jobs;

/// <summary>
/// Runs a queued job and records the outcome. Distinguishes two kinds of failure on purpose:
/// an exception escaping IJobRunner (process couldn't start, disk full, etc.) is treated as
/// transient/infra and rethrown so Hangfire's AutomaticRetry picks it up; a runner that
/// returns a completed-but-unsuccessful outcome (e.g. ffmpeg exited non-zero) is a permanent
/// business failure — recorded as Failed, no retry, since retrying won't change the result.
/// Pushes a notification after every status transition so connected clients (Phase 5, SignalR)
/// don't have to poll.
/// </summary>
public sealed class JobExecutionService : IJobExecutionService
{
    private readonly IJobRepository _repository;
    private readonly IJobProgressNotifier _notifier;
    private readonly ILogger<JobExecutionService> _logger;
    private readonly IReadOnlyDictionary<JobType, IJobRunner> _runners;

    public JobExecutionService(
        IJobRepository repository,
        IJobProgressNotifier notifier,
        IEnumerable<IJobRunner> runners,
        ILogger<JobExecutionService> logger)
    {
        _repository = repository;
        _notifier = notifier;
        _logger = logger;
        _runners = runners.ToDictionary(r => r.Handles);
    }

    public async Task ExecuteAsync(Guid jobId, CancellationToken cancellationToken)
    {
        var job = await _repository.GetAsync(jobId, cancellationToken);
        if (job is null)
        {
            _logger.LogWarning("Job {JobId} not found; skipping execution.", jobId);
            return;
        }

        if (!_runners.TryGetValue(job.Type, out var runner))
        {
            job.MarkFailed($"No runner registered for job type '{job.Type}'.", DateTimeOffset.UtcNow);
            await _repository.UpdateAsync(job, cancellationToken);
            await _notifier.NotifyJobUpdatedAsync(JobStatusDto.FromDomain(job), cancellationToken);
            return;
        }

        job.MarkRunning(DateTimeOffset.UtcNow);
        await _repository.UpdateAsync(job, cancellationToken);
        await _notifier.NotifyJobUpdatedAsync(JobStatusDto.FromDomain(job), cancellationToken);

        JobRunOutcome outcome;
        try
        {
            outcome = await runner.RunAsync(job.Parameters, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Job {JobId} threw while running; will be retried by Hangfire.", jobId);
            job.MarkFailed(ex.Message, DateTimeOffset.UtcNow);
            await _repository.UpdateAsync(job, cancellationToken);
            await _notifier.NotifyJobUpdatedAsync(JobStatusDto.FromDomain(job), cancellationToken);
            throw;
        }

        if (outcome.Success && outcome.OutputPath is not null)
        {
            job.MarkSucceeded(outcome.OutputPath, DateTimeOffset.UtcNow);
        }
        else
        {
            job.MarkFailed(outcome.ErrorMessage ?? "Job failed with no error message.", DateTimeOffset.UtcNow);
        }

        await _repository.UpdateAsync(job, cancellationToken);
        await _notifier.NotifyJobUpdatedAsync(JobStatusDto.FromDomain(job), cancellationToken);
    }
}

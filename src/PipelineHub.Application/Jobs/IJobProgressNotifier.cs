namespace PipelineHub.Application.Jobs;

/// <summary>
/// Port for pushing job status changes to connected clients. SignalR is the real
/// implementation (Api layer, Phase 5) — Application only knows "a status changed".
/// </summary>
public interface IJobProgressNotifier
{
    Task NotifyJobUpdatedAsync(JobStatusDto status, CancellationToken cancellationToken);
}

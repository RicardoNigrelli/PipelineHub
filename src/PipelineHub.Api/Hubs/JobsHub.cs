using Microsoft.AspNetCore.SignalR;

namespace PipelineHub.Api.Hubs;

/// <summary>
/// Clients either watch everything (dashboard: join "all-jobs") or a single job's detail
/// view (join "job-{id}"). JobExecutionService pushes to both groups on every transition,
/// so a client only needs to join whichever group matches what it's showing.
/// </summary>
public sealed class JobsHub : Hub
{
    public const string AllJobsGroup = "all-jobs";

    public static string JobGroup(Guid jobId) => $"job-{jobId}";

    public Task WatchAllJobs() => Groups.AddToGroupAsync(Context.ConnectionId, AllJobsGroup);

    public Task WatchJob(Guid jobId) => Groups.AddToGroupAsync(Context.ConnectionId, JobGroup(jobId));
}

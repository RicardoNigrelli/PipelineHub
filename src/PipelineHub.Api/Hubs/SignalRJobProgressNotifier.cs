using Microsoft.AspNetCore.SignalR;
using PipelineHub.Application.Jobs;

namespace PipelineHub.Api.Hubs;

public sealed class SignalRJobProgressNotifier : IJobProgressNotifier
{
    private const string EventName = "jobUpdated";

    private readonly IHubContext<JobsHub> _hub;

    public SignalRJobProgressNotifier(IHubContext<JobsHub> hub) => _hub = hub;

    public async Task NotifyJobUpdatedAsync(JobStatusDto status, CancellationToken cancellationToken)
    {
        await _hub.Clients.Group(JobsHub.AllJobsGroup).SendAsync(EventName, status, cancellationToken);
        await _hub.Clients.Group(JobsHub.JobGroup(status.Id)).SendAsync(EventName, status, cancellationToken);
    }
}

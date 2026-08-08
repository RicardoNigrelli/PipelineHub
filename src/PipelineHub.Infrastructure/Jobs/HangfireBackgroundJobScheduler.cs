using Hangfire;
using PipelineHub.Application.Jobs;

namespace PipelineHub.Infrastructure.Jobs;

public sealed class HangfireBackgroundJobScheduler : IBackgroundJobScheduler
{
    private readonly IBackgroundJobClient _client;

    public HangfireBackgroundJobScheduler(IBackgroundJobClient client) => _client = client;

    public void ScheduleExecution(Guid jobId) =>
        _client.Enqueue<IJobExecutionService>(x => x.ExecuteAsync(jobId, CancellationToken.None));
}

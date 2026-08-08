using MediatR;
using PipelineHub.Domain;

namespace PipelineHub.Application.Jobs;

/// <summary>
/// Persists the job as Queued and hands it to the background scheduler, then returns
/// immediately — the actual run happens out of request scope via IJobExecutionService,
/// dispatched by Hangfire (Infrastructure). This is the point of Phase 4: the caller no
/// longer blocks on ffmpeg/whisper/Remotion finishing.
/// </summary>
public sealed class EnqueueJobCommandHandler : IRequestHandler<EnqueueJobCommand, Guid>
{
    private readonly IJobRepository _repository;
    private readonly IBackgroundJobScheduler _scheduler;

    public EnqueueJobCommandHandler(IJobRepository repository, IBackgroundJobScheduler scheduler)
    {
        _repository = repository;
        _scheduler = scheduler;
    }

    public async Task<Guid> Handle(EnqueueJobCommand request, CancellationToken cancellationToken)
    {
        var job = new Job(Guid.NewGuid(), request.Type, request.Parameters, DateTimeOffset.UtcNow);
        await _repository.AddAsync(job, cancellationToken);

        _scheduler.ScheduleExecution(job.Id);

        return job.Id;
    }
}

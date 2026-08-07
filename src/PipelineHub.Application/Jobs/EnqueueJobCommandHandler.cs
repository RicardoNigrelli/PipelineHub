using MediatR;
using PipelineHub.Domain;

namespace PipelineHub.Application.Jobs;

/// <summary>
/// Phase 1: runs the job synchronously in-process, no queue yet (that's Hangfire, Phase 4).
/// Good enough for a first end-to-end slice; the handler already depends only on the
/// IJobRunner/IJobRepository ports so swapping in a background queue later doesn't touch this file's contract.
/// </summary>
public sealed class EnqueueJobCommandHandler : IRequestHandler<EnqueueJobCommand, Guid>
{
    private readonly IJobRepository _repository;
    private readonly IReadOnlyDictionary<JobType, IJobRunner> _runners;

    public EnqueueJobCommandHandler(IJobRepository repository, IEnumerable<IJobRunner> runners)
    {
        _repository = repository;
        _runners = runners.ToDictionary(r => r.Handles);
    }

    public async Task<Guid> Handle(EnqueueJobCommand request, CancellationToken cancellationToken)
    {
        var job = new Job(Guid.NewGuid(), request.Type, request.Parameters, DateTimeOffset.UtcNow);
        await _repository.AddAsync(job, cancellationToken);

        if (!_runners.TryGetValue(request.Type, out var runner))
        {
            job.MarkFailed($"No runner registered for job type '{request.Type}'.", DateTimeOffset.UtcNow);
            await _repository.UpdateAsync(job, cancellationToken);
            return job.Id;
        }

        job.MarkRunning(DateTimeOffset.UtcNow);
        await _repository.UpdateAsync(job, cancellationToken);

        var outcome = await runner.RunAsync(request.Parameters, cancellationToken);

        if (outcome.Success && outcome.OutputPath is not null)
        {
            job.MarkSucceeded(outcome.OutputPath, DateTimeOffset.UtcNow);
        }
        else
        {
            job.MarkFailed(outcome.ErrorMessage ?? "Job failed with no error message.", DateTimeOffset.UtcNow);
        }

        await _repository.UpdateAsync(job, cancellationToken);
        return job.Id;
    }
}

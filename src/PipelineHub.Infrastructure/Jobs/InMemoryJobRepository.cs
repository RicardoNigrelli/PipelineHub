using System.Collections.Concurrent;
using PipelineHub.Application.Jobs;
using PipelineHub.Domain;

namespace PipelineHub.Infrastructure.Jobs;

/// <summary>
/// Phase 1 stand-in for persistence. Replaced by an EF Core + Postgres repository in Phase 3 —
/// Application only depends on IJobRepository, so that swap won't touch command/query code.
/// </summary>
public sealed class InMemoryJobRepository : IJobRepository
{
    private readonly ConcurrentDictionary<Guid, Job> _jobs = new();

    public Task AddAsync(Job job, CancellationToken cancellationToken)
    {
        _jobs[job.Id] = job;
        return Task.CompletedTask;
    }

    public Task<Job?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        _jobs.TryGetValue(id, out var job);
        return Task.FromResult(job);
    }

    public Task UpdateAsync(Job job, CancellationToken cancellationToken)
    {
        _jobs[job.Id] = job;
        return Task.CompletedTask;
    }
}

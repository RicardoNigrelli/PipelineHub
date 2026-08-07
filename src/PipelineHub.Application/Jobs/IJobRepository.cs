using PipelineHub.Domain;

namespace PipelineHub.Application.Jobs;

public interface IJobRepository
{
    Task AddAsync(Job job, CancellationToken cancellationToken);

    Task<Job?> GetAsync(Guid id, CancellationToken cancellationToken);

    Task UpdateAsync(Job job, CancellationToken cancellationToken);
}

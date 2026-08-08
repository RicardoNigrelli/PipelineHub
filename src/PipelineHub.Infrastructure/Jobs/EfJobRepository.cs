using Microsoft.EntityFrameworkCore;
using PipelineHub.Application.Jobs;
using PipelineHub.Domain;
using PipelineHub.Infrastructure.Persistence;

namespace PipelineHub.Infrastructure.Jobs;

public sealed class EfJobRepository : IJobRepository
{
    private readonly PipelineHubDbContext _db;

    public EfJobRepository(PipelineHubDbContext db) => _db = db;

    public async Task AddAsync(Job job, CancellationToken cancellationToken)
    {
        _db.Jobs.Add(job);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public Task<Job?> GetAsync(Guid id, CancellationToken cancellationToken) =>
        _db.Jobs.AsNoTracking().FirstOrDefaultAsync(j => j.Id == id, cancellationToken);

    public async Task UpdateAsync(Job job, CancellationToken cancellationToken)
    {
        // The instance is already tracked from AddAsync within the same request scope in the
        // common case; Update() is a safe no-op there and correctly re-attaches otherwise.
        _db.Jobs.Update(job);
        await _db.SaveChangesAsync(cancellationToken);
    }
}

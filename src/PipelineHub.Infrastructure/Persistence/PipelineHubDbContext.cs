using Microsoft.EntityFrameworkCore;
using PipelineHub.Domain;

namespace PipelineHub.Infrastructure.Persistence;

public sealed class PipelineHubDbContext : DbContext
{
    public PipelineHubDbContext(DbContextOptions<PipelineHubDbContext> options) : base(options)
    {
    }

    public DbSet<Job> Jobs => Set<Job>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PipelineHubDbContext).Assembly);
    }
}

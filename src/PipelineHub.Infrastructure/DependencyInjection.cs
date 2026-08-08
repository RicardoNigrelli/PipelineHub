using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PipelineHub.Application.Jobs;
using PipelineHub.Infrastructure.Jobs;
using PipelineHub.Infrastructure.Persistence;

namespace PipelineHub.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string repoRoot, string connectionString)
    {
        services.AddDbContext<PipelineHubDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IJobRepository, EfJobRepository>();
        services.AddSingleton<IJobRunner>(sp => ActivatorUtilities.CreateInstance<SampleFfmpegJobRunner>(sp, repoRoot));

        return services;
    }
}

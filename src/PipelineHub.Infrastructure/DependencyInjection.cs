using Microsoft.Extensions.DependencyInjection;
using PipelineHub.Application.Jobs;
using PipelineHub.Infrastructure.Jobs;

namespace PipelineHub.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string repoRoot)
    {
        services.AddSingleton<IJobRepository, InMemoryJobRepository>();
        services.AddSingleton<IJobRunner>(sp => ActivatorUtilities.CreateInstance<SampleFfmpegJobRunner>(sp, repoRoot));

        return services;
    }
}

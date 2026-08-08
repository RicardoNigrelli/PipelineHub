using Hangfire;
using Hangfire.PostgreSql;
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

        // Separate schema from the app's own tables (Hangfire owns/migrates "hangfire.*" itself).
        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UsePostgreSqlStorage(options => options.UseNpgsqlConnection(connectionString), new PostgreSqlStorageOptions { SchemaName = "hangfire" })
            .UseFilter(new AutomaticRetryAttribute { Attempts = 3 }));
        services.AddHangfireServer();
        services.AddScoped<IBackgroundJobScheduler, HangfireBackgroundJobScheduler>();

        return services;
    }
}

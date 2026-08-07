using PipelineHub.Domain;

namespace PipelineHub.Application.Jobs;

public sealed record JobRunOutcome(bool Success, string? OutputPath, string? ErrorMessage);

/// <summary>
/// Port for executing a job's real work. Infrastructure provides one implementation per
/// JobType — the sample ffmpeg runner ships in this repo; video-lab/reel-lab adapters are
/// registered separately via local (gitignored) configuration, never committed.
/// </summary>
public interface IJobRunner
{
    JobType Handles { get; }

    Task<JobRunOutcome> RunAsync(IReadOnlyDictionary<string, string> parameters, CancellationToken cancellationToken);
}

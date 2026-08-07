using PipelineHub.Domain;

namespace PipelineHub.Application.Jobs;

public sealed record JobStatusDto(
    Guid Id,
    JobType Type,
    JobStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? StartedAt,
    DateTimeOffset? CompletedAt,
    string? ResultOutputPath,
    string? ErrorMessage)
{
    public static JobStatusDto FromDomain(Job job) => new(
        job.Id,
        job.Type,
        job.Status,
        job.CreatedAt,
        job.StartedAt,
        job.CompletedAt,
        job.ResultOutputPath,
        job.ErrorMessage);
}

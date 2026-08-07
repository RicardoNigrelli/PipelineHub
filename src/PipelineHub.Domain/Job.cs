namespace PipelineHub.Domain;

public sealed class Job
{
    public Guid Id { get; }
    public JobType Type { get; }
    public IReadOnlyDictionary<string, string> Parameters { get; }
    public JobStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset? StartedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public string? ResultOutputPath { get; private set; }
    public string? ErrorMessage { get; private set; }

    public Job(Guid id, JobType type, IReadOnlyDictionary<string, string> parameters, DateTimeOffset createdAt)
    {
        Id = id;
        Type = type;
        Parameters = parameters;
        CreatedAt = createdAt;
        Status = JobStatus.Queued;
    }

    public void MarkRunning(DateTimeOffset startedAt)
    {
        Status = JobStatus.Running;
        StartedAt = startedAt;
    }

    public void MarkSucceeded(string outputPath, DateTimeOffset completedAt)
    {
        Status = JobStatus.Succeeded;
        ResultOutputPath = outputPath;
        CompletedAt = completedAt;
    }

    public void MarkFailed(string errorMessage, DateTimeOffset completedAt)
    {
        Status = JobStatus.Failed;
        ErrorMessage = errorMessage;
        CompletedAt = completedAt;
    }
}

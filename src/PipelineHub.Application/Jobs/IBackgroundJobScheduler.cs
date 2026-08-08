namespace PipelineHub.Application.Jobs;

/// <summary>
/// Port over whatever queues background work (Hangfire in Infrastructure). Keeps Application
/// free of any queue-specific types so the queue implementation can be swapped without
/// touching command handlers.
/// </summary>
public interface IBackgroundJobScheduler
{
    void ScheduleExecution(Guid jobId);
}

namespace BuildingBlocks.Application.BackgroundJobs;

/// <summary>
/// Service for scheduling and managing background jobs
/// Abstraction over Hangfire
/// </summary>
public interface IBackgroundJobService
{
    /// <summary>
    /// Enqueue a fire-and-forget job
    /// Executes once, as soon as possible
    /// </summary>
    string Enqueue<T>(System.Linq.Expressions.Expression<Action<T>> methodCall);

    /// <summary>
    /// Schedule a job to run after a delay
    /// </summary>
    string Schedule<T>(System.Linq.Expressions.Expression<Action<T>> methodCall, TimeSpan delay);

    /// <summary>
    /// Schedule a recurring job with cron expression
    /// Example: "0 0 * * *" = daily at midnight
    /// </summary>
    void AddOrUpdate<T>(string jobId, System.Linq.Expressions.Expression<Action<T>> methodCall, string cronExpression);

    /// <summary>
    /// Delete a scheduled or recurring job
    /// </summary>
    bool Delete(string jobId);
}


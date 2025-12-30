using BuildingBlocks.Application.BackgroundJobs;
using Hangfire;

namespace BuildingBlocks.Infrastructure.BackgroundJobs;

/// <summary>
/// Hangfire implementation of background job service
/// </summary>
public class HangfireBackgroundJobService : IBackgroundJobService
{
    public string Enqueue<T>(System.Linq.Expressions.Expression<Action<T>> methodCall)
    {
        return BackgroundJob.Enqueue(methodCall);
    }

    public string Schedule<T>(System.Linq.Expressions.Expression<Action<T>> methodCall, TimeSpan delay)
    {
        return BackgroundJob.Schedule(methodCall, delay);
    }

    public void AddOrUpdate<T>(string jobId, System.Linq.Expressions.Expression<Action<T>> methodCall, string cronExpression)
    {
        RecurringJob.AddOrUpdate(jobId, methodCall, cronExpression);
    }

    public bool Delete(string jobId)
    {
        return BackgroundJob.Delete(jobId);
    }
}


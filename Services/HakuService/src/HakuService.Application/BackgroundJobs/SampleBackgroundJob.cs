using Microsoft.Extensions.Logging;

namespace HakuService.Application.BackgroundJobs;

/// <summary>
/// Example background job
/// Demonstrates how to create and schedule jobs
/// </summary>
public class SampleBackgroundJob
{
    private readonly ILogger<SampleBackgroundJob> _logger;

    public SampleBackgroundJob(ILogger<SampleBackgroundJob> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Example: Clean up old cache entries
    /// Can be scheduled as recurring job: "0 2 * * *" (2 AM daily)
    /// </summary>
    public async Task CleanupOldCacheEntries()
    {
        _logger.LogInformation("Starting cache cleanup job");
        
        // Cleanup logic here
        await Task.Delay(1000); // Simulate work
        
        _logger.LogInformation("Cache cleanup completed");
    }

    /// <summary>
    /// Example: Send notification emails
    /// Can be enqueued as fire-and-forget job
    /// </summary>
    public async Task SendNotificationEmails(List<int> userIds)
    {
        _logger.LogInformation("Sending notification emails to {Count} users", userIds.Count);
        
        // Email sending logic here
        await Task.Delay(1000); // Simulate work
        
        _logger.LogInformation("Notification emails sent");
    }
}


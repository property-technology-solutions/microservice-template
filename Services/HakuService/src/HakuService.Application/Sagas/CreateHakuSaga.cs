using BuildingBlocks.Application.Sagas;
using BuildingBlocks.Infrastructure.Http;
using HakuService.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace HakuService.Application.Sagas;

/// <summary>
/// Example saga: Create Haku with external service calls
/// Demonstrates compensating transactions for HTTP-based microservices
/// </summary>
public class CreateHakuWithNotificationSaga
{
    // Step 1: Create Haku in database
    public class CreateHakuStep : SagaStep
    {
        private readonly IApplicationDbContext _context;
        private readonly ILogger<CreateHakuStep> _logger;
        private int _createdHakuId;

        public CreateHakuStep(IApplicationDbContext context, ILogger<CreateHakuStep> logger)
        {
            _context = context;
            _logger = logger;
        }

        public override async Task ExecuteAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Creating Haku in database");
            // Create logic here
            _createdHakuId = 123; // Simulated
            await Task.CompletedTask;
        }

        public override async Task CompensateAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogWarning("Compensating: Deleting Haku {HakuId}", _createdHakuId);
            // Delete logic here
            await Task.CompletedTask;
        }
    }

    // Step 2: Send notification to external service
    public class SendNotificationStep : SagaStep
    {
        private readonly IServiceClient _serviceClient;
        private readonly ILogger<SendNotificationStep> _logger;
        private string? _notificationId;

        public SendNotificationStep(IServiceClient serviceClient, ILogger<SendNotificationStep> logger)
        {
            _serviceClient = serviceClient;
            _logger = logger;
        }

        public override async Task ExecuteAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Sending notification to external service");
            
            // Call notification service via HTTP
            // var response = await _serviceClient.PostAsync<NotificationRequest, NotificationResponse>(...);
            _notificationId = "notif-123"; // Simulated
            
            await Task.CompletedTask;
        }

        public override async Task CompensateAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogWarning("Compensating: Canceling notification {NotificationId}", _notificationId);
            
            // Cancel notification via HTTP DELETE
            // await _serviceClient.DeleteAsync("http://notification-service", $"api/notifications/{_notificationId}");
            
            await Task.CompletedTask;
        }
    }

    // Step 3: Update search index (external service)
    public class UpdateSearchIndexStep : SagaStep
    {
        private readonly IServiceClient _serviceClient;
        private readonly ILogger<UpdateSearchIndexStep> _logger;

        public UpdateSearchIndexStep(IServiceClient serviceClient, ILogger<UpdateSearchIndexStep> logger)
        {
            _serviceClient = serviceClient;
            _logger = logger;
        }

        public override async Task ExecuteAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Updating search index");
            // Call search service via HTTP
            await Task.CompletedTask;
        }

        public override async Task CompensateAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogWarning("Compensating: Removing from search index");
            // Remove from search service via HTTP DELETE
            await Task.CompletedTask;
        }
    }
}


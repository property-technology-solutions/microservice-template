using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Application.Sagas;

/// <summary>
/// Simple saga orchestrator for HTTP-based microservices
/// Manages distributed transactions with compensating actions
/// </summary>
public class SagaOrchestrator
{
    private readonly ILogger<SagaOrchestrator> _logger;
    private readonly Stack<SagaStep> _executedSteps = new();

    public SagaOrchestrator(ILogger<SagaOrchestrator> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Execute saga steps in sequence
    /// If any step fails, compensate all previous steps in reverse order
    /// </summary>
    public async Task<SagaResult> ExecuteAsync(
        List<SagaStep> steps,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting saga execution with {StepCount} steps", steps.Count);

        try
        {
            // Execute steps in order
            foreach (var step in steps)
            {
                _logger.LogInformation("Executing step: {StepType}", step.GetType().Name);
                
                await step.ExecuteAsync(cancellationToken);
                _executedSteps.Push(step);
                
                _logger.LogInformation("Step completed: {StepType}", step.GetType().Name);
            }

            _logger.LogInformation("Saga completed successfully");
            return SagaResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Saga failed, starting compensation");
            await CompensateAsync(cancellationToken);
            return SagaResult.Failure(ex.Message);
        }
    }

    /// <summary>
    /// Compensate all executed steps in reverse order
    /// </summary>
    private async Task CompensateAsync(CancellationToken cancellationToken)
    {
        _logger.LogWarning("Compensating {StepCount} executed steps", _executedSteps.Count);

        while (_executedSteps.Count > 0)
        {
            var step = _executedSteps.Pop();
            
            try
            {
                _logger.LogInformation("Compensating step: {StepType}", step.GetType().Name);
                await step.CompensateAsync(cancellationToken);
                _logger.LogInformation("Compensation completed: {StepType}", step.GetType().Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Compensation failed for step: {StepType}", step.GetType().Name);
                // Continue compensating other steps
            }
        }

        _logger.LogWarning("Saga compensation completed");
    }
}

/// <summary>
/// Result of saga execution
/// </summary>
public class SagaResult
{
    public bool IsSuccess { get; private init; }
    public string? ErrorMessage { get; private init; }

    public static SagaResult Success() => new() { IsSuccess = true };
    public static SagaResult Failure(string message) => new() { IsSuccess = false, ErrorMessage = message };
}


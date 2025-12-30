namespace BuildingBlocks.Application.Sagas;

/// <summary>
/// Represents a compensating action in a saga
/// Rollback/undo operation for a saga step
/// </summary>
public interface ICompensatingAction
{
    /// <summary>
    /// Execute the compensating action
    /// Should undo the effects of the original action
    /// </summary>
    Task ExecuteAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Base class for saga steps with compensation
/// </summary>
public abstract class SagaStep
{
    /// <summary>
    /// Execute the forward action
    /// </summary>
    public abstract Task ExecuteAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Execute the compensating action (rollback)
    /// </summary>
    public abstract Task CompensateAsync(CancellationToken cancellationToken = default);
}


namespace BuildingBlocks.Infrastructure;

/// <summary>
/// Abstraction for system time
/// Allows for easier testing by mocking time
/// </summary>
public interface IClock
{
    DateTime UtcNow { get; }
    DateTime Now { get; }
}

/// <summary>
/// Production implementation using system time
/// </summary>
public class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
    public DateTime Now => DateTime.Now;
}


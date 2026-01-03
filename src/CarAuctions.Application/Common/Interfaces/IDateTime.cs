namespace CarAuctions.Application.Common.Interfaces;

/// <summary>
/// Abstraction for date/time operations to enable testing.
/// </summary>
public interface IDateTime
{
    /// <summary>
    /// Gets the current UTC date and time.
    /// </summary>
    DateTime UtcNow { get; }
}

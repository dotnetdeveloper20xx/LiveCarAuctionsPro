namespace CarAuctions.Application.Common.Interfaces;

/// <summary>
/// Abstraction for the application database context.
/// </summary>
public interface IApplicationDbContext
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

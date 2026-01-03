using CarAuctions.Domain.Aggregates.Auctions;
using CarAuctions.Domain.Aggregates.Bids;
using CarAuctions.Domain.Aggregates.Users;
using CarAuctions.Domain.Aggregates.Vehicles;
using Microsoft.EntityFrameworkCore;

namespace CarAuctions.Application.Common.Interfaces;

/// <summary>
/// Abstraction for the application database context.
/// </summary>
public interface IApplicationDbContext
{
    DbSet<Auction> Auctions { get; }
    DbSet<Vehicle> Vehicles { get; }
    DbSet<Bid> Bids { get; }
    DbSet<User> Users { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

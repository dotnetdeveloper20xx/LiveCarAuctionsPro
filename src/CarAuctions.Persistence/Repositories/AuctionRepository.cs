using CarAuctions.Application.Common.Interfaces;
using CarAuctions.Domain.Aggregates.Auctions;
using CarAuctions.Domain.Aggregates.Users;
using CarAuctions.Domain.Aggregates.Vehicles;
using Microsoft.EntityFrameworkCore;

namespace CarAuctions.Persistence.Repositories;

public class AuctionRepository : Repository<Auction, AuctionId>, IAuctionRepository
{
    public AuctionRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Auction>> GetActiveAuctionsAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(a => a.Status == AuctionStatus.Active)
            .OrderBy(a => a.EndTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Auction>> GetAuctionsBySellerAsync(
        UserId sellerId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(a => a.SellerId == sellerId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Auction>> GetAuctionsByVehicleAsync(
        VehicleId vehicleId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(a => a.VehicleId == vehicleId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Auction>> GetEndingSoonAsync(
        TimeSpan window,
        CancellationToken cancellationToken = default)
    {
        var endBefore = DateTime.UtcNow.Add(window);
        return await DbSet
            .Where(a => a.Status == AuctionStatus.Active && a.EndTime <= endBefore)
            .OrderBy(a => a.EndTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Auction>> GetScheduledToStartAsync(
        DateTime before,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(a => a.Status == AuctionStatus.Scheduled && a.StartTime <= before)
            .OrderBy(a => a.StartTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Auction>> GetEndedButNotClosedAsync(
        DateTime endedBefore,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(a => a.Status == AuctionStatus.Active && a.EndTime <= endedBefore)
            .ToListAsync(cancellationToken);
    }
}

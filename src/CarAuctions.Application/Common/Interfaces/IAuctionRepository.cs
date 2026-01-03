using CarAuctions.Domain.Aggregates.Auctions;
using CarAuctions.Domain.Aggregates.Users;
using CarAuctions.Domain.Aggregates.Vehicles;

namespace CarAuctions.Application.Common.Interfaces;

public interface IAuctionRepository : IRepository<Auction, AuctionId>
{
    Task<IReadOnlyList<Auction>> GetActiveAuctionsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Auction>> GetAuctionsBySellerAsync(UserId sellerId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Auction>> GetAuctionsByVehicleAsync(VehicleId vehicleId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Auction>> GetEndingSoonAsync(TimeSpan window, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Auction>> GetScheduledToStartAsync(DateTime before, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Auction>> GetEndedButNotClosedAsync(DateTime endedBefore, CancellationToken cancellationToken = default);
}

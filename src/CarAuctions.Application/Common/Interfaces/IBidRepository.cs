using CarAuctions.Domain.Aggregates.Auctions;
using CarAuctions.Domain.Aggregates.Bids;
using CarAuctions.Domain.Aggregates.Users;

namespace CarAuctions.Application.Common.Interfaces;

public interface IBidRepository : IRepository<Bid, BidId>
{
    Task<IReadOnlyList<Bid>> GetByAuctionAsync(AuctionId auctionId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Bid>> GetByBidderAsync(UserId bidderId, CancellationToken cancellationToken = default);
    Task<Bid?> GetHighestBidAsync(AuctionId auctionId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Bid>> GetActiveBidsByBidderAsync(UserId bidderId, CancellationToken cancellationToken = default);
    Task<int> GetBidCountAsync(AuctionId auctionId, CancellationToken cancellationToken = default);
    Task<decimal> GetOutstandingBidsTotalAsync(UserId bidderId, CancellationToken cancellationToken = default);
}

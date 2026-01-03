using CarAuctions.Application.Common.Interfaces;
using CarAuctions.Domain.Aggregates.Auctions;
using CarAuctions.Domain.Aggregates.Bids;
using CarAuctions.Domain.Aggregates.Users;
using Microsoft.EntityFrameworkCore;

namespace CarAuctions.Persistence.Repositories;

public class BidRepository : Repository<Bid, BidId>, IBidRepository
{
    public BidRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Bid>> GetByAuctionAsync(
        AuctionId auctionId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(b => b.AuctionId == auctionId)
            .OrderByDescending(b => b.Amount.Amount)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Bid>> GetByBidderAsync(
        UserId bidderId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(b => b.BidderId == bidderId)
            .OrderByDescending(b => b.PlacedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Bid?> GetHighestBidAsync(
        AuctionId auctionId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(b => b.AuctionId == auctionId && b.Status != BidStatus.Withdrawn)
            .OrderByDescending(b => b.Amount.Amount)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Bid>> GetActiveBidsByBidderAsync(
        UserId bidderId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(b => b.BidderId == bidderId && b.Status == BidStatus.Active)
            .OrderByDescending(b => b.PlacedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetBidCountAsync(
        AuctionId auctionId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .CountAsync(b => b.AuctionId == auctionId && b.Status != BidStatus.Withdrawn, cancellationToken);
    }

    public async Task<decimal> GetOutstandingBidsTotalAsync(
        UserId bidderId,
        CancellationToken cancellationToken = default)
    {
        // Get winning bids on active auctions (these are committed amounts)
        var winningBidsTotal = await Context.Set<Auction>()
            .Where(a => a.Status == AuctionStatus.Active && a.WinningBidId != null)
            .Join(
                DbSet.Where(b => b.BidderId == bidderId && b.Status == BidStatus.Winning),
                auction => auction.WinningBidId,
                bid => bid.Id,
                (auction, bid) => bid.Amount.Amount)
            .SumAsync(cancellationToken);

        return winningBidsTotal;
    }
}

using CarAuctions.Application.Common.Interfaces;
using CarAuctions.Domain.Aggregates.Auctions;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CarAuctions.Application.Features.Bids.Queries.GetBidHistory;

public class GetBidHistoryQueryHandler : IRequestHandler<GetBidHistoryQuery, ErrorOr<IReadOnlyList<BidDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetBidHistoryQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<IReadOnlyList<BidDto>>> Handle(
        GetBidHistoryQuery request,
        CancellationToken cancellationToken)
    {
        var auctionId = new AuctionId(request.AuctionId);

        var auctionExists = await _context.Auctions
            .AnyAsync(a => a.Id == auctionId, cancellationToken);

        if (!auctionExists)
        {
            return Error.NotFound("Auction.NotFound", "Auction not found");
        }

        var bids = await _context.Bids
            .Where(b => b.AuctionId == auctionId)
            .OrderByDescending(b => b.PlacedAt)
            .Take(request.Limit)
            .Select(b => new BidDto
            {
                Id = b.Id.Value,
                AuctionId = b.AuctionId.Value,
                BidderId = b.BidderId.Value,
                BidderName = string.Empty,
                Amount = b.Amount.Amount,
                Currency = b.Amount.Currency,
                Status = b.Status.ToString(),
                PlacedAt = b.PlacedAt,
                IsProxyBid = b.IsProxyBid,
                MaxProxyAmount = b.MaxProxyAmount != null ? b.MaxProxyAmount.Amount : null
            })
            .ToListAsync(cancellationToken);

        return bids;
    }
}

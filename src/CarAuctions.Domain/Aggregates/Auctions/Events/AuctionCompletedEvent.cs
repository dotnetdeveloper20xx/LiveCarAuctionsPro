using CarAuctions.Domain.Aggregates.Bids;
using CarAuctions.Domain.Aggregates.Users;
using CarAuctions.Domain.Common;

namespace CarAuctions.Domain.Aggregates.Auctions.Events;

public sealed class AuctionCompletedEvent : DomainEventBase
{
    public AuctionId AuctionId { get; }
    public BidId WinningBidId { get; }
    public UserId WinnerId { get; }
    public Money FinalPrice { get; }
    public bool WasBuyNow { get; }

    public AuctionCompletedEvent(
        AuctionId auctionId,
        BidId winningBidId,
        UserId winnerId,
        Money finalPrice,
        bool wasBuyNow)
    {
        AuctionId = auctionId;
        WinningBidId = winningBidId;
        WinnerId = winnerId;
        FinalPrice = finalPrice;
        WasBuyNow = wasBuyNow;
    }
}

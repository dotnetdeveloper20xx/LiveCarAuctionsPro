using CarAuctions.Domain.Aggregates.Auctions;
using CarAuctions.Domain.Aggregates.Users;
using CarAuctions.Domain.Common;

namespace CarAuctions.Domain.Aggregates.Bids.Events;

public sealed class BidWonEvent : DomainEventBase
{
    public BidId BidId { get; }
    public AuctionId AuctionId { get; }
    public UserId WinnerId { get; }
    public Money WinningAmount { get; }

    public BidWonEvent(BidId bidId, AuctionId auctionId, UserId winnerId, Money winningAmount)
    {
        BidId = bidId;
        AuctionId = auctionId;
        WinnerId = winnerId;
        WinningAmount = winningAmount;
    }
}

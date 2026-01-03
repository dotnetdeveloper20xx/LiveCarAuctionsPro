using CarAuctions.Domain.Common;

namespace CarAuctions.Domain.Aggregates.Auctions.Events;

public sealed class AuctionEndedNoSaleEvent : DomainEventBase
{
    public AuctionId AuctionId { get; }
    public Money HighestBid { get; }
    public Money? ReservePrice { get; }

    public AuctionEndedNoSaleEvent(AuctionId auctionId, Money highestBid, Money? reservePrice)
    {
        AuctionId = auctionId;
        HighestBid = highestBid;
        ReservePrice = reservePrice;
    }
}

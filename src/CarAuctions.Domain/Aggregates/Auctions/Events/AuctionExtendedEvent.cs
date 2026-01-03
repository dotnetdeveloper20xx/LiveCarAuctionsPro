using CarAuctions.Domain.Common;

namespace CarAuctions.Domain.Aggregates.Auctions.Events;

public sealed class AuctionExtendedEvent : DomainEventBase
{
    public AuctionId AuctionId { get; }
    public DateTime NewEndTime { get; }
    public int ExtensionCount { get; }

    public AuctionExtendedEvent(AuctionId auctionId, DateTime newEndTime, int extensionCount)
    {
        AuctionId = auctionId;
        NewEndTime = newEndTime;
        ExtensionCount = extensionCount;
    }
}

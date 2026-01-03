using CarAuctions.Domain.Common;

namespace CarAuctions.Domain.Aggregates.Auctions;

public sealed class AuctionId : StronglyTypedId<AuctionId>
{
    public AuctionId(Guid value) : base(value)
    {
    }

    public static AuctionId CreateUnique() => new(Guid.NewGuid());

    public static AuctionId Create(Guid value) => new(value);
}

using CarAuctions.Domain.Common;

namespace CarAuctions.Domain.Aggregates.Bids;

public sealed class BidId : StronglyTypedId<BidId>
{
    public BidId(Guid value) : base(value)
    {
    }

    public static BidId CreateUnique() => new(Guid.NewGuid());

    public static BidId Create(Guid value) => new(value);
}

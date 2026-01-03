using CarAuctions.Domain.Common;

namespace CarAuctions.Domain.Aggregates.Payments;

public sealed class PaymentId : StronglyTypedId<PaymentId>
{
    public PaymentId(Guid value) : base(value)
    {
    }

    public static PaymentId CreateUnique() => new(Guid.NewGuid());

    public static PaymentId Create(Guid value) => new(value);
}

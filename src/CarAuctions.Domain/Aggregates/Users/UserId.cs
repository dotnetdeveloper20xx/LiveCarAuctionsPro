using CarAuctions.Domain.Common;

namespace CarAuctions.Domain.Aggregates.Users;

public sealed class UserId : StronglyTypedId<UserId>
{
    public UserId(Guid value) : base(value)
    {
    }

    public static UserId CreateUnique() => new(Guid.NewGuid());

    public static UserId Create(Guid value) => new(value);
}

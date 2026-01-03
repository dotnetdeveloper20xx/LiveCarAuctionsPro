namespace CarAuctions.Domain.Common;

/// <summary>
/// Base class for strongly-typed identifiers using GUID.
/// </summary>
/// <typeparam name="TId">The derived strongly-typed ID type.</typeparam>
public abstract class StronglyTypedId<TId> : ValueObject
    where TId : StronglyTypedId<TId>
{
    public Guid Value { get; }

    protected StronglyTypedId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("Id cannot be empty.", nameof(value));
        }

        Value = value;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(StronglyTypedId<TId> id) => id.Value;
}

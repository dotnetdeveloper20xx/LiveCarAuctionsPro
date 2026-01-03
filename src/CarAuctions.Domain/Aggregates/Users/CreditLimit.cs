using CarAuctions.Domain.Common;
using ErrorOr;

namespace CarAuctions.Domain.Aggregates.Users;

/// <summary>
/// Value object representing a user's credit limit for bidding.
/// </summary>
public sealed class CreditLimit : ValueObject
{
    public Money TotalLimit { get; private set; } = null!;
    public Money UsedAmount { get; private set; } = null!;
    public Money AvailableAmount => TotalLimit - UsedAmount;

    // EF Core constructor
    private CreditLimit() { }

    private CreditLimit(Money totalLimit, Money usedAmount)
    {
        TotalLimit = totalLimit;
        UsedAmount = usedAmount;
    }

    public static CreditLimit Create(Money totalLimit)
    {
        return new CreditLimit(totalLimit, Money.Zero(totalLimit.Currency));
    }

    public static CreditLimit CreateWithUsed(Money totalLimit, Money usedAmount)
    {
        return new CreditLimit(totalLimit, usedAmount);
    }

    public ErrorOr<CreditLimit> Reserve(Money amount)
    {
        if (amount > AvailableAmount)
            return Error.Validation("CreditLimit.Insufficient",
                $"Insufficient credit. Available: {AvailableAmount}, Requested: {amount}");

        return new CreditLimit(TotalLimit, UsedAmount + amount);
    }

    public CreditLimit Release(Money amount)
    {
        var newUsed = UsedAmount - amount;
        if (newUsed.Amount < 0)
            newUsed = Money.Zero(TotalLimit.Currency);

        return new CreditLimit(TotalLimit, newUsed);
    }

    public CreditLimit UpdateLimit(Money newLimit)
    {
        return new CreditLimit(newLimit, UsedAmount);
    }

    public bool CanAfford(Money amount) => AvailableAmount >= amount;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return TotalLimit;
        yield return UsedAmount;
    }

    public override string ToString() => $"Available: {AvailableAmount} / {TotalLimit}";
}

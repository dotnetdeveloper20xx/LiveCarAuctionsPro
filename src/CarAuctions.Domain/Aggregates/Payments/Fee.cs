using CarAuctions.Domain.Common;

namespace CarAuctions.Domain.Aggregates.Payments;

public sealed class Fee : ValueObject
{
    public FeeType Type { get; private set; }
    public Money Amount { get; private set; } = null!;
    public decimal Rate { get; private set; }
    public string Description { get; private set; } = null!;

    // EF Core constructor
    private Fee() { }

    private Fee(FeeType type, Money amount, decimal rate, string description)
    {
        Type = type;
        Amount = amount;
        Rate = rate;
        Description = description;
    }

    public static Fee Calculate(Money baseAmount, FeeType type, decimal rate)
    {
        var feeAmount = Math.Round(baseAmount.Amount * rate, 2, MidpointRounding.AwayFromZero);
        var description = type switch
        {
            FeeType.BuyerPremium => $"Buyer Premium ({rate:P0})",
            FeeType.SellerFee => $"Seller Fee ({rate:P0})",
            FeeType.ListingFee => "Listing Fee",
            FeeType.TransactionFee => "Transaction Fee",
            _ => "Fee"
        };

        return new Fee(type, Money.Create(feeAmount, baseAmount.Currency), rate, description);
    }

    public static Fee Fixed(FeeType type, Money amount, string description)
    {
        return new Fee(type, amount, 0, description);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Type;
        yield return Amount;
        yield return Rate;
    }
}

public enum FeeType
{
    BuyerPremium,
    SellerFee,
    ListingFee,
    TransactionFee,
    StorageFee,
    InspectionFee
}

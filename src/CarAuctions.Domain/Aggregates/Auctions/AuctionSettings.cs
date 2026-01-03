using CarAuctions.Domain.Common;

namespace CarAuctions.Domain.Aggregates.Auctions;

/// <summary>
/// Configuration settings for an auction.
/// </summary>
public sealed class AuctionSettings : ValueObject
{
    public TimeSpan AntiSnipingWindow { get; private set; }
    public TimeSpan AntiSnipingExtension { get; private set; }
    public Money MinimumBidIncrement { get; private set; } = null!;
    public bool AllowProxyBidding { get; private set; }
    public int MaxExtensions { get; private set; }
    public bool RequireDeposit { get; private set; }
    public Money? DepositAmount { get; private set; }

    // EF Core constructor
    private AuctionSettings() { }

    private AuctionSettings(
        TimeSpan antiSnipingWindow,
        TimeSpan antiSnipingExtension,
        Money minimumBidIncrement,
        bool allowProxyBidding,
        int maxExtensions,
        bool requireDeposit,
        Money? depositAmount)
    {
        AntiSnipingWindow = antiSnipingWindow;
        AntiSnipingExtension = antiSnipingExtension;
        MinimumBidIncrement = minimumBidIncrement;
        AllowProxyBidding = allowProxyBidding;
        MaxExtensions = maxExtensions;
        RequireDeposit = requireDeposit;
        DepositAmount = depositAmount;
    }

    public static AuctionSettings Default(string currency = "USD") => new(
        TimeSpan.FromMinutes(2),
        TimeSpan.FromMinutes(5),
        Money.Create(100, currency),
        allowProxyBidding: true,
        maxExtensions: 3,
        requireDeposit: false,
        depositAmount: null);

    public static AuctionSettings Create(
        TimeSpan antiSnipingWindow,
        TimeSpan antiSnipingExtension,
        Money minimumBidIncrement,
        bool allowProxyBidding = true,
        int maxExtensions = 3,
        bool requireDeposit = false,
        Money? depositAmount = null)
    {
        return new AuctionSettings(
            antiSnipingWindow,
            antiSnipingExtension,
            minimumBidIncrement,
            allowProxyBidding,
            maxExtensions,
            requireDeposit,
            depositAmount);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return AntiSnipingWindow;
        yield return AntiSnipingExtension;
        yield return MinimumBidIncrement;
        yield return AllowProxyBidding;
        yield return MaxExtensions;
        yield return RequireDeposit;
        yield return DepositAmount;
    }
}

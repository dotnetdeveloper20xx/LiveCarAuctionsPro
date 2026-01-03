using CarAuctions.Domain.Common;

namespace CarAuctions.Domain.Aggregates.Auctions;

/// <summary>
/// Configuration settings for an auction.
/// </summary>
public sealed class AuctionSettings : ValueObject
{
    public TimeSpan AntiSnipingWindow { get; }
    public TimeSpan AntiSnipingExtension { get; }
    public Money MinimumBidIncrement { get; }
    public bool AllowProxyBidding { get; }
    public int MaxExtensions { get; }
    public bool RequireDeposit { get; }
    public Money? DepositAmount { get; }

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

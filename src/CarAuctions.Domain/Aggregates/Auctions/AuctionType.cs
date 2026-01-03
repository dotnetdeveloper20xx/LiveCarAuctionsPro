namespace CarAuctions.Domain.Aggregates.Auctions;

public enum AuctionType
{
    /// <summary>
    /// Live auction with real-time bidding.
    /// </summary>
    Live = 0,

    /// <summary>
    /// Timed auction with set end time.
    /// </summary>
    Timed = 1,

    /// <summary>
    /// Buy Now only - no bidding.
    /// </summary>
    BuyNow = 2,

    /// <summary>
    /// Sealed bid auction - bids revealed at end.
    /// </summary>
    SealedBid = 3
}

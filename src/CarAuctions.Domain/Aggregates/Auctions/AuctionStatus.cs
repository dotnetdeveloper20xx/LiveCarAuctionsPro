namespace CarAuctions.Domain.Aggregates.Auctions;

public enum AuctionStatus
{
    /// <summary>
    /// Auction is being set up.
    /// </summary>
    Draft = 0,

    /// <summary>
    /// Auction is scheduled but not yet started.
    /// </summary>
    Scheduled = 1,

    /// <summary>
    /// Auction is currently accepting bids.
    /// </summary>
    Active = 2,

    /// <summary>
    /// Auction ended with a winning bid.
    /// </summary>
    Completed = 3,

    /// <summary>
    /// Auction ended without meeting reserve.
    /// </summary>
    EndedNoSale = 4,

    /// <summary>
    /// Auction was cancelled.
    /// </summary>
    Cancelled = 5,

    /// <summary>
    /// Auction is paused (admin action).
    /// </summary>
    Paused = 6
}

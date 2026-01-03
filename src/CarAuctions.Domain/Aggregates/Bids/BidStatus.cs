namespace CarAuctions.Domain.Aggregates.Bids;

public enum BidStatus
{
    /// <summary>
    /// Bid is active and valid.
    /// </summary>
    Active = 0,

    /// <summary>
    /// Bid has been outbid.
    /// </summary>
    Outbid = 1,

    /// <summary>
    /// Bid is the winning bid.
    /// </summary>
    Winning = 2,

    /// <summary>
    /// Bid was withdrawn.
    /// </summary>
    Withdrawn = 3,

    /// <summary>
    /// Bid was rejected (validation failed).
    /// </summary>
    Rejected = 4
}

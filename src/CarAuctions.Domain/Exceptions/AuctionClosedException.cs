namespace CarAuctions.Domain.Exceptions;

/// <summary>
/// Exception thrown when an operation is attempted on a closed auction.
/// </summary>
public sealed class AuctionClosedException : DomainException
{
    public Guid AuctionId { get; }

    public AuctionClosedException(Guid auctionId)
        : base($"Auction {auctionId} is closed and cannot accept new bids.")
    {
        AuctionId = auctionId;
    }
}

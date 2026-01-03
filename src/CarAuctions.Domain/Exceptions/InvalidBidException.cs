namespace CarAuctions.Domain.Exceptions;

/// <summary>
/// Exception thrown when a bid is invalid.
/// </summary>
public sealed class InvalidBidException : DomainException
{
    public decimal BidAmount { get; }
    public decimal MinimumRequired { get; }

    public InvalidBidException(string message) : base(message)
    {
    }

    public InvalidBidException(decimal bidAmount, decimal minimumRequired)
        : base($"Bid amount {bidAmount:C} is below the minimum required {minimumRequired:C}.")
    {
        BidAmount = bidAmount;
        MinimumRequired = minimumRequired;
    }
}

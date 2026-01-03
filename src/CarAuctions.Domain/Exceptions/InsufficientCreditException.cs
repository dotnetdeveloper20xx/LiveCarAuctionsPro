namespace CarAuctions.Domain.Exceptions;

/// <summary>
/// Exception thrown when a user has insufficient credit to place a bid.
/// </summary>
public sealed class InsufficientCreditException : DomainException
{
    public Guid UserId { get; }
    public decimal RequestedAmount { get; }
    public decimal AvailableCredit { get; }

    public InsufficientCreditException(Guid userId, decimal requestedAmount, decimal availableCredit)
        : base($"User {userId} has insufficient credit. Requested: {requestedAmount:C}, Available: {availableCredit:C}.")
    {
        UserId = userId;
        RequestedAmount = requestedAmount;
        AvailableCredit = availableCredit;
    }
}

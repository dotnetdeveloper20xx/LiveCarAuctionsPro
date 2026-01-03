using CarAuctions.Domain.Aggregates.Auctions;
using CarAuctions.Domain.Aggregates.Payments.Events;
using CarAuctions.Domain.Aggregates.Users;
using CarAuctions.Domain.Common;
using ErrorOr;

namespace CarAuctions.Domain.Aggregates.Payments;

public sealed class Payment : AggregateRoot<PaymentId>
{
    public AuctionId AuctionId { get; private set; }
    public UserId UserId { get; private set; }
    public Money Amount { get; private set; }
    public Fee BuyerFee { get; private set; }
    public Money TotalAmount { get; private set; }
    public PaymentStatus Status { get; private set; }
    public string? ExternalReference { get; private set; }
    public string? TransactionId { get; private set; }
    public string? FailureReason { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    private readonly List<Invoice> _invoices = new();
    public IReadOnlyList<Invoice> Invoices => _invoices.AsReadOnly();

    private Payment(
        PaymentId id,
        AuctionId auctionId,
        UserId userId,
        Money amount,
        Fee buyerFee,
        DateTime createdAt) : base(id)
    {
        AuctionId = auctionId;
        UserId = userId;
        Amount = amount;
        BuyerFee = buyerFee;
        TotalAmount = Money.Create(amount.Amount + buyerFee.Amount.Amount, amount.Currency);
        Status = PaymentStatus.Pending;
        CreatedAt = createdAt;
    }

    private Payment() : base()
    {
        AuctionId = default!;
        UserId = default!;
        Amount = Money.Zero();
        BuyerFee = default!;
        TotalAmount = Money.Zero();
    }

    public static ErrorOr<Payment> Create(
        AuctionId auctionId,
        UserId userId,
        Money amount,
        Fee buyerFee,
        DateTime createdAt)
    {
        if (amount.Amount <= 0)
            return Error.Validation("Payment.InvalidAmount", "Payment amount must be positive");

        var payment = new Payment(
            PaymentId.CreateUnique(),
            auctionId,
            userId,
            amount,
            buyerFee,
            createdAt);

        payment.RaiseDomainEvent(new PaymentInitiatedEvent(
            payment.Id,
            payment.AuctionId,
            payment.UserId,
            payment.TotalAmount));

        return payment;
    }

    public void SetExternalReference(string reference)
    {
        ExternalReference = reference;
        Status = PaymentStatus.Processing;
    }

    public ErrorOr<Success> Complete(string transactionId, DateTime completedAt)
    {
        if (Status == PaymentStatus.Completed)
            return Error.Conflict("Payment.AlreadyCompleted", "Payment has already been completed");

        if (Status == PaymentStatus.Failed)
            return Error.Conflict("Payment.Failed", "Cannot complete a failed payment");

        TransactionId = transactionId;
        Status = PaymentStatus.Completed;
        CompletedAt = completedAt;

        RaiseDomainEvent(new PaymentCompletedEvent(Id, AuctionId, UserId, TotalAmount));

        return Result.Success;
    }

    public void MarkAsFailed(string reason)
    {
        Status = PaymentStatus.Failed;
        FailureReason = reason;

        RaiseDomainEvent(new PaymentFailedEvent(Id, AuctionId, UserId, reason));
    }

    public ErrorOr<Success> Refund(string reason, DateTime refundedAt)
    {
        if (Status != PaymentStatus.Completed)
            return Error.Conflict("Payment.NotCompleted", "Only completed payments can be refunded");

        Status = PaymentStatus.Refunded;
        FailureReason = reason;

        RaiseDomainEvent(new PaymentRefundedEvent(Id, AuctionId, UserId, TotalAmount, reason));

        return Result.Success;
    }

    public void AddInvoice(Invoice invoice)
    {
        _invoices.Add(invoice);
    }
}

public enum PaymentStatus
{
    Pending,
    Processing,
    Completed,
    Failed,
    Refunded
}

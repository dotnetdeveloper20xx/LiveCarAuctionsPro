using CarAuctions.Domain.Common;

namespace CarAuctions.Domain.Aggregates.Payments;

public sealed class Invoice : BaseEntity<InvoiceId>
{
    public PaymentId PaymentId { get; private set; }
    public string InvoiceNumber { get; private set; }
    public Money Amount { get; private set; }
    public Fee BuyerFee { get; private set; }
    public Money TotalAmount { get; private set; }
    public InvoiceStatus Status { get; private set; }
    public DateTime IssuedAt { get; private set; }
    public DateTime DueDate { get; private set; }
    public DateTime? PaidAt { get; private set; }

    private Invoice(
        InvoiceId id,
        PaymentId paymentId,
        string invoiceNumber,
        Money amount,
        Fee buyerFee,
        DateTime dueDate) : base(id)
    {
        PaymentId = paymentId;
        InvoiceNumber = invoiceNumber;
        Amount = amount;
        BuyerFee = buyerFee;
        TotalAmount = Money.Create(amount.Amount + buyerFee.Amount.Amount, amount.Currency);
        Status = InvoiceStatus.Issued;
        IssuedAt = DateTime.UtcNow;
        DueDate = dueDate;
    }

    private Invoice() : base()
    {
        PaymentId = default!;
        InvoiceNumber = string.Empty;
        Amount = Money.Zero();
        BuyerFee = default!;
        TotalAmount = Money.Zero();
    }

    public static Invoice Create(
        PaymentId paymentId,
        string invoiceNumber,
        Money amount,
        Fee buyerFee,
        DateTime dueDate)
    {
        return new Invoice(
            InvoiceId.CreateUnique(),
            paymentId,
            invoiceNumber,
            amount,
            buyerFee,
            dueDate);
    }

    public void MarkAsPaid(DateTime paidAt)
    {
        Status = InvoiceStatus.Paid;
        PaidAt = paidAt;
    }

    public void Cancel()
    {
        Status = InvoiceStatus.Cancelled;
    }
}

public readonly record struct InvoiceId(Guid Value)
{
    public static InvoiceId CreateUnique() => new(Guid.NewGuid());
}

public enum InvoiceStatus
{
    Issued,
    Paid,
    Overdue,
    Cancelled
}

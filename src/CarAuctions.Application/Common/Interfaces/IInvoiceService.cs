using CarAuctions.Domain.Aggregates.Payments;

namespace CarAuctions.Application.Common.Interfaces;

public interface IInvoiceService
{
    Task<Invoice> GenerateInvoiceAsync(Payment payment, CancellationToken cancellationToken = default);
    Task<byte[]> GenerateInvoicePdfAsync(Guid invoiceId, CancellationToken cancellationToken = default);
    Task SendInvoiceEmailAsync(Guid invoiceId, string recipientEmail, CancellationToken cancellationToken = default);
}

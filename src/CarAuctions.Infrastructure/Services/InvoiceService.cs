using CarAuctions.Application.Common.Interfaces;
using CarAuctions.Domain.Aggregates.Payments;
using Microsoft.Extensions.Logging;
using DomainPayment = CarAuctions.Domain.Aggregates.Payments.Payment;

namespace CarAuctions.Infrastructure.Services;

public class InvoiceService : IInvoiceService
{
    private readonly IApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly ILogger<InvoiceService> _logger;

    public InvoiceService(
        IApplicationDbContext context,
        IEmailService emailService,
        ILogger<InvoiceService> logger)
    {
        _context = context;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<Invoice> GenerateInvoiceAsync(
        DomainPayment payment,
        CancellationToken cancellationToken = default)
    {
        var invoiceNumber = await GenerateInvoiceNumberAsync(cancellationToken);

        var invoice = Invoice.Create(
            payment.Id,
            invoiceNumber,
            payment.Amount,
            payment.BuyerFee,
            DateTime.UtcNow.AddDays(30)); // Due in 30 days

        payment.AddInvoice(invoice);

        _logger.LogInformation(
            "Invoice {InvoiceNumber} generated for payment {PaymentId}",
            invoiceNumber, payment.Id.Value);

        return invoice;
    }

    public Task<byte[]> GenerateInvoicePdfAsync(
        Guid invoiceId,
        CancellationToken cancellationToken = default)
    {
        // In production, use a PDF library like QuestPDF, iTextSharp, or similar
        _logger.LogInformation("Generating PDF for invoice {InvoiceId}", invoiceId);

        // Return a placeholder PDF
        var placeholderContent = $"Invoice {invoiceId}\nGenerated at {DateTime.UtcNow}";
        return Task.FromResult(System.Text.Encoding.UTF8.GetBytes(placeholderContent));
    }

    public async Task SendInvoiceEmailAsync(
        Guid invoiceId,
        string recipientEmail,
        CancellationToken cancellationToken = default)
    {
        var pdfContent = await GenerateInvoicePdfAsync(invoiceId, cancellationToken);

        await _emailService.SendEmailAsync(
            recipientEmail,
            $"Your Invoice #{invoiceId}",
            "Please find your invoice attached. Thank you for your purchase!",
            cancellationToken);

        _logger.LogInformation(
            "Invoice email sent to {Email} for invoice {InvoiceId}",
            recipientEmail, invoiceId);
    }

    private async Task<string> GenerateInvoiceNumberAsync(CancellationToken cancellationToken)
    {
        // Simple invoice number generation: INV-YYYYMMDD-XXXX
        var today = DateTime.UtcNow;
        var prefix = $"INV-{today:yyyyMMdd}-";

        var count = await Task.FromResult(1); // In production, query for today's invoice count
        return $"{prefix}{count:D4}";
    }
}

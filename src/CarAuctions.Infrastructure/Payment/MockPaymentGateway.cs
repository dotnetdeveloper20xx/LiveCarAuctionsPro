using CarAuctions.Application.Common.Interfaces;
using ErrorOr;
using Microsoft.Extensions.Logging;

namespace CarAuctions.Infrastructure.Payment;

public class MockPaymentGateway : IPaymentGateway
{
    private readonly ILogger<MockPaymentGateway> _logger;
    private readonly Dictionary<string, GatewayPaymentStatus> _payments = new();

    public MockPaymentGateway(ILogger<MockPaymentGateway> logger)
    {
        _logger = logger;
    }

    public Task<ErrorOr<string>> InitiatePaymentAsync(
        Guid paymentId,
        string customerEmail,
        decimal amount,
        string currency,
        string description,
        CancellationToken cancellationToken = default)
    {
        var externalReference = $"MOCK_{paymentId:N}_{DateTime.UtcNow:yyyyMMddHHmmss}";

        _logger.LogInformation(
            "Mock payment initiated: {ExternalReference} for {Amount} {Currency} to {Email}",
            externalReference, amount, currency, customerEmail);

        _payments[externalReference] = GatewayPaymentStatus.Pending;

        // In production, this would return a payment URL or session ID
        return Task.FromResult<ErrorOr<string>>(externalReference);
    }

    public Task<ErrorOr<GatewayPaymentStatus>> GetPaymentStatusAsync(
        string externalReference,
        CancellationToken cancellationToken = default)
    {
        if (_payments.TryGetValue(externalReference, out var status))
        {
            return Task.FromResult<ErrorOr<GatewayPaymentStatus>>(status);
        }

        return Task.FromResult<ErrorOr<GatewayPaymentStatus>>(
            Error.NotFound("Payment.NotFound", "Payment not found in gateway"));
    }

    public Task<ErrorOr<string>> RefundPaymentAsync(
        string externalReference,
        decimal amount,
        string reason,
        CancellationToken cancellationToken = default)
    {
        if (!_payments.ContainsKey(externalReference))
        {
            return Task.FromResult<ErrorOr<string>>(
                Error.NotFound("Payment.NotFound", "Payment not found in gateway"));
        }

        var refundReference = $"REFUND_{externalReference}";
        _payments[externalReference] = GatewayPaymentStatus.Refunded;

        _logger.LogInformation(
            "Mock refund processed: {RefundReference} for {Amount}. Reason: {Reason}",
            refundReference, amount, reason);

        return Task.FromResult<ErrorOr<string>>(refundReference);
    }

    // Test helper method to simulate payment completion
    public void SimulatePaymentCompletion(string externalReference, bool success)
    {
        if (_payments.ContainsKey(externalReference))
        {
            _payments[externalReference] = success ? GatewayPaymentStatus.Completed : GatewayPaymentStatus.Failed;
        }
    }
}

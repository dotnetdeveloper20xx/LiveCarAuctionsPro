using ErrorOr;

namespace CarAuctions.Application.Common.Interfaces;

public interface IPaymentGateway
{
    Task<ErrorOr<string>> InitiatePaymentAsync(
        Guid paymentId,
        string customerEmail,
        decimal amount,
        string currency,
        string description,
        CancellationToken cancellationToken = default);

    Task<ErrorOr<GatewayPaymentStatus>> GetPaymentStatusAsync(
        string externalReference,
        CancellationToken cancellationToken = default);

    Task<ErrorOr<string>> RefundPaymentAsync(
        string externalReference,
        decimal amount,
        string reason,
        CancellationToken cancellationToken = default);
}

public enum GatewayPaymentStatus
{
    Pending,
    Processing,
    Completed,
    Failed,
    Refunded
}

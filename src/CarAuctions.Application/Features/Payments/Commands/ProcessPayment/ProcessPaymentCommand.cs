using ErrorOr;
using MediatR;

namespace CarAuctions.Application.Features.Payments.Commands.ProcessPayment;

public record ProcessPaymentCommand(
    Guid PaymentId,
    string ExternalTransactionId,
    bool IsSuccessful,
    string? FailureReason = null
) : IRequest<ErrorOr<Success>>;

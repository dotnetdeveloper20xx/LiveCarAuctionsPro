using ErrorOr;
using MediatR;

namespace CarAuctions.Application.Features.Payments.Commands.InitiatePayment;

public record InitiatePaymentCommand(
    Guid AuctionId,
    Guid WinningBidderId,
    decimal Amount,
    string Currency = "USD"
) : IRequest<ErrorOr<Guid>>;

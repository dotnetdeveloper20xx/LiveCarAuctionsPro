using ErrorOr;
using MediatR;

namespace CarAuctions.Application.Features.Bids.Commands.PlaceBid;

public record PlaceBidCommand(
    Guid AuctionId,
    Guid BidderId,
    decimal Amount,
    string Currency,
    bool IsProxy = false,
    decimal? MaxProxyAmount = null) : IRequest<ErrorOr<Guid>>;

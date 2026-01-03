using ErrorOr;
using MediatR;

namespace CarAuctions.Application.Features.Auctions.Commands.CreateAuction;

public record CreateAuctionCommand(
    string Title,
    string Type,
    Guid VehicleId,
    Guid SellerId,
    decimal StartingPrice,
    string Currency,
    DateTime StartTime,
    DateTime EndTime,
    decimal? ReservePrice,
    decimal? BuyNowPrice,
    string? Description,
    bool IsDealerOnly = false) : IRequest<ErrorOr<Guid>>;

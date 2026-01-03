using ErrorOr;
using MediatR;

namespace CarAuctions.Application.Features.Auctions.Commands.CloseAuction;

public record CloseAuctionCommand(Guid AuctionId) : IRequest<ErrorOr<Success>>;

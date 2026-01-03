using ErrorOr;
using MediatR;

namespace CarAuctions.Application.Features.Auctions.Commands.StartAuction;

public record StartAuctionCommand(Guid AuctionId) : IRequest<ErrorOr<Success>>;

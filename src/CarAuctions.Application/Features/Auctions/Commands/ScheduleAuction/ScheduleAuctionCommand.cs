using ErrorOr;
using MediatR;

namespace CarAuctions.Application.Features.Auctions.Commands.ScheduleAuction;

public record ScheduleAuctionCommand(Guid AuctionId) : IRequest<ErrorOr<Success>>;

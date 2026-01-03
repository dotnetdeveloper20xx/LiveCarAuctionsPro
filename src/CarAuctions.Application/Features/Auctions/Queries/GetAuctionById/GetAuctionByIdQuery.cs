using ErrorOr;
using MediatR;
using CarAuctions.Application.Features.Auctions.Queries.GetAuctions;

namespace CarAuctions.Application.Features.Auctions.Queries.GetAuctionById;

public record GetAuctionByIdQuery(Guid Id) : IRequest<ErrorOr<AuctionDto>>;

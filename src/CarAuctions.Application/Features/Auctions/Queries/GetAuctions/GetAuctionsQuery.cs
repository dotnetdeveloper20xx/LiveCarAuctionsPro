using CarAuctions.Application.Common.Models;
using ErrorOr;
using MediatR;

namespace CarAuctions.Application.Features.Auctions.Queries.GetAuctions;

public record GetAuctionsQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string? Status = null,
    string? Type = null,
    bool? DealerOnly = null) : IRequest<ErrorOr<PaginatedList<AuctionDto>>>;

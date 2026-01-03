using ErrorOr;
using MediatR;

namespace CarAuctions.Application.Features.Bids.Queries.GetBidHistory;

public record GetBidHistoryQuery(
    Guid AuctionId,
    int Limit = 50) : IRequest<ErrorOr<IReadOnlyList<BidDto>>>;

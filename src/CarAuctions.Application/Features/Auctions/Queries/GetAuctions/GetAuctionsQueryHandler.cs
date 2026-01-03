using CarAuctions.Application.Common.Interfaces;
using CarAuctions.Application.Common.Models;
using CarAuctions.Domain.Aggregates.Auctions;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CarAuctions.Application.Features.Auctions.Queries.GetAuctions;

public class GetAuctionsQueryHandler : IRequestHandler<GetAuctionsQuery, ErrorOr<PaginatedList<AuctionDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetAuctionsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<PaginatedList<AuctionDto>>> Handle(
        GetAuctionsQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.Auctions.AsNoTracking();

        if (!string.IsNullOrEmpty(request.Status) &&
            Enum.TryParse<AuctionStatus>(request.Status, true, out var status))
        {
            query = query.Where(a => a.Status == status);
        }

        if (!string.IsNullOrEmpty(request.Type) &&
            Enum.TryParse<AuctionType>(request.Type, true, out var type))
        {
            query = query.Where(a => a.Type == type);
        }

        if (request.DealerOnly.HasValue)
        {
            query = query.Where(a => a.IsDealerOnly == request.DealerOnly.Value);
        }

        query = query.OrderByDescending(a => a.StartTime);

        var projectedQuery = query.Select(a => new AuctionDto
        {
            Id = a.Id.Value,
            Title = a.Title,
            Type = a.Type.ToString(),
            Status = a.Status.ToString(),
            VehicleId = a.VehicleId.Value,
            SellerId = a.SellerId.Value,
            StartingPrice = a.StartingPrice.Amount,
            Currency = a.StartingPrice.Currency,
            ReservePrice = a.ReservePrice != null ? a.ReservePrice.Amount : null,
            BuyNowPrice = a.BuyNowPrice != null ? a.BuyNowPrice.Amount : null,
            CurrentHighBid = a.CurrentHighBid.Amount,
            StartTime = a.StartTime,
            EndTime = a.EndTime,
            ActualEndTime = a.ActualEndTime,
            BidCount = a.BidCount,
            IsDealerOnly = a.IsDealerOnly,
            Description = a.Description
        });

        return await PaginatedList<AuctionDto>.CreateAsync(
            projectedQuery,
            request.PageNumber,
            request.PageSize,
            cancellationToken);
    }
}

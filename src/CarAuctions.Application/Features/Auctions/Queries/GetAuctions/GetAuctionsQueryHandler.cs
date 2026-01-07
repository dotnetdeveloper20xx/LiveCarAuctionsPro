using CarAuctions.Application.Common.Interfaces;
using CarAuctions.Application.Common.Models;
using CarAuctions.Domain.Aggregates.Auctions;
using CarAuctions.Domain.Aggregates.Vehicles;
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

        // Join with vehicles to get vehicle info including images
        var auctionsWithVehicles = from auction in query
                                   join vehicle in _context.Vehicles.Include(v => v.Images)
                                   on auction.VehicleId equals vehicle.Id
                                   select new { auction, vehicle };

        var projectedQuery = auctionsWithVehicles.Select(av => new AuctionDto
        {
            Id = av.auction.Id.Value,
            Title = av.auction.Title,
            Type = av.auction.Type.ToString(),
            Status = av.auction.Status.ToString(),
            VehicleId = av.auction.VehicleId.Value,
            SellerId = av.auction.SellerId.Value,
            StartingPrice = av.auction.StartingPrice.Amount,
            Currency = av.auction.StartingPrice.Currency,
            ReservePrice = av.auction.ReservePrice != null ? av.auction.ReservePrice.Amount : null,
            BuyNowPrice = av.auction.BuyNowPrice != null ? av.auction.BuyNowPrice.Amount : null,
            CurrentHighBid = av.auction.CurrentHighBid.Amount,
            StartTime = av.auction.StartTime,
            EndTime = av.auction.EndTime,
            ActualEndTime = av.auction.ActualEndTime,
            BidCount = av.auction.BidCount,
            IsDealerOnly = av.auction.IsDealerOnly,
            Description = av.auction.Description,
            Vehicle = new VehicleSummaryDto
            {
                Id = av.vehicle.Id.Value,
                VIN = av.vehicle.VIN.Value,
                Make = av.vehicle.Make,
                Model = av.vehicle.Model,
                Year = av.vehicle.Year,
                Mileage = av.vehicle.Mileage.Value,
                ExteriorColor = av.vehicle.ExteriorColor,
                ImageUrl = av.vehicle.Images.FirstOrDefault(i => i.IsPrimary) != null
                    ? av.vehicle.Images.First(i => i.IsPrimary).Url
                    : av.vehicle.Images.FirstOrDefault() != null
                        ? av.vehicle.Images.First().Url
                        : null
            }
        });

        return await PaginatedList<AuctionDto>.CreateAsync(
            projectedQuery,
            request.PageNumber,
            request.PageSize,
            cancellationToken);
    }
}

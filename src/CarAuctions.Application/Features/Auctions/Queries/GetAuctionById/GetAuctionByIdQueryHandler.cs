using CarAuctions.Application.Common.Interfaces;
using CarAuctions.Application.Features.Auctions.Queries.GetAuctions;
using CarAuctions.Domain.Aggregates.Auctions;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CarAuctions.Application.Features.Auctions.Queries.GetAuctionById;

public class GetAuctionByIdQueryHandler : IRequestHandler<GetAuctionByIdQuery, ErrorOr<AuctionDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAuctionByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<AuctionDto>> Handle(
        GetAuctionByIdQuery request,
        CancellationToken cancellationToken)
    {
        var auctionId = new AuctionId(request.Id);

        var auction = await _context.Auctions
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == auctionId, cancellationToken);

        if (auction is null)
        {
            return Error.NotFound("Auction.NotFound", "Auction not found");
        }

        return new AuctionDto
        {
            Id = auction.Id.Value,
            Title = auction.Title,
            Type = auction.Type.ToString(),
            Status = auction.Status.ToString(),
            VehicleId = auction.VehicleId.Value,
            SellerId = auction.SellerId.Value,
            StartingPrice = auction.StartingPrice.Amount,
            Currency = auction.StartingPrice.Currency,
            ReservePrice = auction.ReservePrice?.Amount,
            BuyNowPrice = auction.BuyNowPrice?.Amount,
            CurrentHighBid = auction.CurrentHighBid.Amount,
            StartTime = auction.StartTime,
            EndTime = auction.EndTime,
            ActualEndTime = auction.ActualEndTime,
            BidCount = auction.BidCount,
            IsDealerOnly = auction.IsDealerOnly,
            Description = auction.Description
        };
    }
}

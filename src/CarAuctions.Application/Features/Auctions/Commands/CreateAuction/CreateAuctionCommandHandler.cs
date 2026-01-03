using CarAuctions.Application.Common.Interfaces;
using CarAuctions.Domain.Aggregates.Auctions;
using CarAuctions.Domain.Aggregates.Users;
using CarAuctions.Domain.Aggregates.Vehicles;
using CarAuctions.Domain.Common;
using ErrorOr;
using MediatR;

namespace CarAuctions.Application.Features.Auctions.Commands.CreateAuction;

public class CreateAuctionCommandHandler : IRequestHandler<CreateAuctionCommand, ErrorOr<Guid>>
{
    private readonly IAuctionRepository _auctionRepository;
    private readonly IVehicleRepository _vehicleRepository;

    public CreateAuctionCommandHandler(
        IAuctionRepository auctionRepository,
        IVehicleRepository vehicleRepository)
    {
        _auctionRepository = auctionRepository;
        _vehicleRepository = vehicleRepository;
    }

    public async Task<ErrorOr<Guid>> Handle(
        CreateAuctionCommand request,
        CancellationToken cancellationToken)
    {
        var vehicleId = new VehicleId(request.VehicleId);
        var vehicle = await _vehicleRepository.GetByIdAsync(vehicleId, cancellationToken);

        if (vehicle is null)
        {
            return Error.NotFound("Vehicle.NotFound", "Vehicle not found");
        }

        if (vehicle.Status != VehicleStatus.Listed)
        {
            return Error.Validation("Vehicle.NotAvailable", "Vehicle is not available for auction");
        }

        if (!Enum.TryParse<AuctionType>(request.Type, true, out var auctionType))
        {
            return Error.Validation("Auction.InvalidType", "Invalid auction type");
        }

        var startingPrice = Money.Create(request.StartingPrice, request.Currency);
        var reservePrice = request.ReservePrice.HasValue
            ? Money.Create(request.ReservePrice.Value, request.Currency)
            : null;
        var buyNowPrice = request.BuyNowPrice.HasValue
            ? Money.Create(request.BuyNowPrice.Value, request.Currency)
            : null;

        var settings = AuctionSettings.Default(request.Currency);

        var auctionResult = Auction.Create(
            request.Title,
            auctionType,
            vehicleId,
            new UserId(request.SellerId),
            startingPrice,
            request.StartTime,
            request.EndTime,
            settings,
            reservePrice,
            buyNowPrice,
            request.Description,
            request.IsDealerOnly);

        if (auctionResult.IsError)
        {
            return auctionResult.Errors;
        }

        var auction = auctionResult.Value;
        await _auctionRepository.AddAsync(auction, cancellationToken);
        await _auctionRepository.SaveChangesAsync(cancellationToken);

        return auction.Id.Value;
    }
}

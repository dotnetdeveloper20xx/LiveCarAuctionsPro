using CarAuctions.Application.Common.Interfaces;
using CarAuctions.Domain.Aggregates.Users;
using CarAuctions.Domain.Aggregates.Vehicles;
using ErrorOr;
using MediatR;

namespace CarAuctions.Application.Features.Vehicles.Commands.RegisterVehicle;

public class RegisterVehicleCommandHandler : IRequestHandler<RegisterVehicleCommand, ErrorOr<Guid>>
{
    private readonly IVehicleRepository _vehicleRepository;

    public RegisterVehicleCommandHandler(IVehicleRepository vehicleRepository)
    {
        _vehicleRepository = vehicleRepository;
    }

    public async Task<ErrorOr<Guid>> Handle(
        RegisterVehicleCommand request,
        CancellationToken cancellationToken)
    {
        var vinResult = VIN.Create(request.VIN);
        if (vinResult.IsError)
        {
            return vinResult.Errors;
        }

        var existingVehicle = await _vehicleRepository.GetByVinAsync(vinResult.Value, cancellationToken);
        if (existingVehicle is not null)
        {
            return Error.Conflict("Vehicle.VinExists", "A vehicle with this VIN already exists");
        }

        var mileageResult = Mileage.Create(request.Mileage);
        if (mileageResult.IsError)
        {
            return mileageResult.Errors;
        }

        if (!Enum.TryParse<TitleStatus>(request.TitleStatus, true, out var titleStatus))
        {
            return Error.Validation("Vehicle.InvalidTitleStatus", "Invalid title status");
        }

        var vehicleResult = Vehicle.Create(
            vinResult.Value,
            request.Make,
            request.Model,
            request.Year,
            mileageResult.Value,
            new UserId(request.OwnerId),
            titleStatus,
            request.ExteriorColor,
            request.InteriorColor,
            request.EngineType,
            request.Transmission,
            request.FuelType);

        if (vehicleResult.IsError)
        {
            return vehicleResult.Errors;
        }

        var vehicle = vehicleResult.Value;
        await _vehicleRepository.AddAsync(vehicle, cancellationToken);

        return vehicle.Id.Value;
    }
}

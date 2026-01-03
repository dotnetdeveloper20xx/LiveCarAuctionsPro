using ErrorOr;
using MediatR;

namespace CarAuctions.Application.Features.Vehicles.Commands.RegisterVehicle;

public record RegisterVehicleCommand(
    string VIN,
    string Make,
    string Model,
    int Year,
    int Mileage,
    Guid OwnerId,
    string TitleStatus,
    string? ExteriorColor = null,
    string? InteriorColor = null,
    string? EngineType = null,
    string? Transmission = null,
    string? FuelType = null) : IRequest<ErrorOr<Guid>>;

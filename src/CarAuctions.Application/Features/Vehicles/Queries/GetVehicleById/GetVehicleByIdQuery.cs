using CarAuctions.Application.Features.Vehicles.Queries.GetVehicles;
using ErrorOr;
using MediatR;

namespace CarAuctions.Application.Features.Vehicles.Queries.GetVehicleById;

public record GetVehicleByIdQuery(Guid Id) : IRequest<ErrorOr<VehicleDto>>;

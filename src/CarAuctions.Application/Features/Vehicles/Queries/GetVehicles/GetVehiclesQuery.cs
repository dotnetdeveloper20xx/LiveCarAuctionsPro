using CarAuctions.Application.Common.Models;
using ErrorOr;
using MediatR;

namespace CarAuctions.Application.Features.Vehicles.Queries.GetVehicles;

public record GetVehiclesQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string? Status = null,
    string? Make = null,
    int? YearFrom = null,
    int? YearTo = null) : IRequest<ErrorOr<PaginatedList<VehicleDto>>>;

using CarAuctions.Application.Common.Interfaces;
using CarAuctions.Application.Features.Vehicles.Queries.GetVehicles;
using CarAuctions.Domain.Aggregates.Vehicles;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CarAuctions.Application.Features.Vehicles.Queries.GetVehicleById;

public class GetVehicleByIdQueryHandler : IRequestHandler<GetVehicleByIdQuery, ErrorOr<VehicleDto>>
{
    private readonly IApplicationDbContext _context;

    public GetVehicleByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<VehicleDto>> Handle(
        GetVehicleByIdQuery request,
        CancellationToken cancellationToken)
    {
        var vehicleId = new VehicleId(request.Id);

        var vehicle = await _context.Vehicles
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == vehicleId, cancellationToken);

        if (vehicle is null)
        {
            return Error.NotFound("Vehicle.NotFound", "Vehicle not found");
        }

        return new VehicleDto
        {
            Id = vehicle.Id.Value,
            VIN = vehicle.VIN.Value,
            Make = vehicle.Make,
            Model = vehicle.Model,
            Year = vehicle.Year,
            Mileage = vehicle.Mileage.Value,
            Status = vehicle.Status.ToString(),
            TitleStatus = vehicle.TitleStatus.ToString(),
            OwnerId = vehicle.OwnerId.Value,
            ExteriorColor = vehicle.ExteriorColor,
            InteriorColor = vehicle.InteriorColor,
            EngineType = vehicle.EngineType,
            Transmission = vehicle.Transmission,
            FuelType = vehicle.FuelType,
            IsSalvage = vehicle.IsSalvage,
            ConditionReport = vehicle.ConditionReport != null ? new ConditionReportDto
            {
                OverallGrade = vehicle.ConditionReport.OverallGrade.ToString(),
                ExteriorGrade = vehicle.ConditionReport.ExteriorGrade.ToString(),
                InteriorGrade = vehicle.ConditionReport.InteriorGrade.ToString(),
                MechanicalGrade = vehicle.ConditionReport.MechanicalGrade.ToString(),
                InspectedAt = vehicle.ConditionReport.InspectedAt,
                Notes = vehicle.ConditionReport.Notes
            } : null
        };
    }
}

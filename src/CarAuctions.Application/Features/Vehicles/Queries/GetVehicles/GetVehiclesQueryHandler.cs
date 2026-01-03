using CarAuctions.Application.Common.Interfaces;
using CarAuctions.Application.Common.Models;
using CarAuctions.Domain.Aggregates.Vehicles;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CarAuctions.Application.Features.Vehicles.Queries.GetVehicles;

public class GetVehiclesQueryHandler : IRequestHandler<GetVehiclesQuery, ErrorOr<PaginatedList<VehicleDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetVehiclesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<PaginatedList<VehicleDto>>> Handle(
        GetVehiclesQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.Vehicles.AsNoTracking();

        if (!string.IsNullOrEmpty(request.Status) &&
            Enum.TryParse<VehicleStatus>(request.Status, true, out var status))
        {
            query = query.Where(v => v.Status == status);
        }

        if (!string.IsNullOrEmpty(request.Make))
        {
            query = query.Where(v => v.Make.Contains(request.Make));
        }

        if (request.YearFrom.HasValue)
        {
            query = query.Where(v => v.Year >= request.YearFrom.Value);
        }

        if (request.YearTo.HasValue)
        {
            query = query.Where(v => v.Year <= request.YearTo.Value);
        }

        query = query.OrderByDescending(v => v.CreatedAt);

        var projectedQuery = query.Select(v => new VehicleDto
        {
            Id = v.Id.Value,
            VIN = v.VIN.Value,
            Make = v.Make,
            Model = v.Model,
            Year = v.Year,
            Mileage = v.Mileage.Value,
            Status = v.Status.ToString(),
            TitleStatus = v.TitleStatus.ToString(),
            OwnerId = v.OwnerId.Value,
            ExteriorColor = v.ExteriorColor,
            InteriorColor = v.InteriorColor,
            EngineType = v.EngineType,
            Transmission = v.Transmission,
            FuelType = v.FuelType,
            IsSalvage = v.IsSalvage,
            ConditionReport = v.ConditionReport != null ? new ConditionReportDto
            {
                OverallGrade = v.ConditionReport.OverallGrade.ToString(),
                ExteriorGrade = v.ConditionReport.ExteriorGrade.ToString(),
                InteriorGrade = v.ConditionReport.InteriorGrade.ToString(),
                MechanicalGrade = v.ConditionReport.MechanicalGrade.ToString(),
                InspectedAt = v.ConditionReport.InspectedAt,
                Notes = v.ConditionReport.Notes
            } : null
        });

        return await PaginatedList<VehicleDto>.CreateAsync(
            projectedQuery,
            request.PageNumber,
            request.PageSize,
            cancellationToken);
    }
}

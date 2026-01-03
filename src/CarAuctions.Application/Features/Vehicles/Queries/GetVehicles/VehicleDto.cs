namespace CarAuctions.Application.Features.Vehicles.Queries.GetVehicles;

public record VehicleDto
{
    public Guid Id { get; init; }
    public string VIN { get; init; } = string.Empty;
    public string Make { get; init; } = string.Empty;
    public string Model { get; init; } = string.Empty;
    public int Year { get; init; }
    public int Mileage { get; init; }
    public string Status { get; init; } = string.Empty;
    public string TitleStatus { get; init; } = string.Empty;
    public Guid OwnerId { get; init; }
    public string? ExteriorColor { get; init; }
    public string? InteriorColor { get; init; }
    public string? EngineType { get; init; }
    public string? Transmission { get; init; }
    public string? FuelType { get; init; }
    public bool IsSalvage { get; init; }
    public ConditionReportDto? ConditionReport { get; init; }
}

public record ConditionReportDto
{
    public string OverallGrade { get; init; } = string.Empty;
    public string ExteriorGrade { get; init; } = string.Empty;
    public string InteriorGrade { get; init; } = string.Empty;
    public string MechanicalGrade { get; init; } = string.Empty;
    public DateTime InspectedAt { get; init; }
    public string? Notes { get; init; }
}

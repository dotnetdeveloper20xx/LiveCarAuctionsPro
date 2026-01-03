using CarAuctions.Domain.Aggregates.Users;
using CarAuctions.Domain.Aggregates.Vehicles.Events;
using CarAuctions.Domain.Common;
using CarAuctions.Domain.Exceptions;
using ErrorOr;

namespace CarAuctions.Domain.Aggregates.Vehicles;

/// <summary>
/// Vehicle aggregate root representing a car in the auction system.
/// </summary>
public sealed class Vehicle : AggregateRoot<VehicleId>
{
    private readonly List<VehicleImage> _images = new();

    public VIN VIN { get; private set; }
    public string Make { get; private set; }
    public string Model { get; private set; }
    public int Year { get; private set; }
    public Mileage Mileage { get; private set; }
    public VehicleStatus Status { get; private set; }
    public TitleStatus TitleStatus { get; private set; }
    public ConditionReport? ConditionReport { get; private set; }
    public UserId OwnerId { get; private set; }
    public string? ExteriorColor { get; private set; }
    public string? InteriorColor { get; private set; }
    public string? EngineType { get; private set; }
    public string? Transmission { get; private set; }
    public string? FuelType { get; private set; }
    public string? Description { get; private set; }
    public bool IsSalvage { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public IReadOnlyList<VehicleImage> Images => _images.AsReadOnly();

    private Vehicle(
        VehicleId id,
        VIN vin,
        string make,
        string model,
        int year,
        Mileage mileage,
        UserId ownerId,
        TitleStatus titleStatus,
        string? exteriorColor,
        string? interiorColor,
        string? engineType,
        string? transmission,
        string? fuelType,
        string? description,
        bool isSalvage) : base(id)
    {
        VIN = vin;
        Make = make;
        Model = model;
        Year = year;
        Mileage = mileage;
        OwnerId = ownerId;
        TitleStatus = titleStatus;
        ExteriorColor = exteriorColor;
        InteriorColor = interiorColor;
        EngineType = engineType;
        Transmission = transmission;
        FuelType = fuelType;
        Description = description;
        IsSalvage = isSalvage || titleStatus == TitleStatus.Salvage;
        Status = VehicleStatus.Draft;
        CreatedAt = DateTime.UtcNow;
    }

    private Vehicle() : base()
    {
        VIN = null!;
        Make = string.Empty;
        Model = string.Empty;
        Mileage = null!;
        OwnerId = null!;
    }

    public static ErrorOr<Vehicle> Create(
        VIN vin,
        string make,
        string model,
        int year,
        Mileage mileage,
        UserId ownerId,
        TitleStatus titleStatus = TitleStatus.Clean,
        string? exteriorColor = null,
        string? interiorColor = null,
        string? engineType = null,
        string? transmission = null,
        string? fuelType = null,
        string? description = null,
        bool isSalvage = false)
    {
        if (string.IsNullOrWhiteSpace(make))
            return Error.Validation("Vehicle.MakeRequired", "Make is required.");

        if (string.IsNullOrWhiteSpace(model))
            return Error.Validation("Vehicle.ModelRequired", "Model is required.");

        if (year < 1900 || year > DateTime.UtcNow.Year + 2)
            return Error.Validation("Vehicle.InvalidYear", "Year must be between 1900 and next year.");

        var vehicle = new Vehicle(
            VehicleId.CreateUnique(),
            vin,
            make.Trim(),
            model.Trim(),
            year,
            mileage,
            ownerId,
            titleStatus,
            exteriorColor?.Trim(),
            interiorColor?.Trim(),
            engineType?.Trim(),
            transmission?.Trim(),
            fuelType?.Trim(),
            description?.Trim(),
            isSalvage);

        vehicle.RaiseDomainEvent(new VehicleRegisteredEvent(vehicle.Id, vehicle.VIN.Value, vehicle.OwnerId));

        return vehicle;
    }

    public ErrorOr<Success> AddConditionReport(ConditionReport report)
    {
        if (Status != VehicleStatus.Draft && Status != VehicleStatus.PendingInspection)
            return Error.Conflict("Vehicle.InvalidStatus", "Vehicle must be in Draft or PendingInspection status.");

        ConditionReport = report;
        Status = VehicleStatus.Inspected;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new VehicleInspectedEvent(Id, report.OverallGrade, report.InspectorId));

        return Result.Success;
    }

    public ErrorOr<Success> AddImage(string url, ImageType type, bool isPrimary = false)
    {
        if (string.IsNullOrWhiteSpace(url))
            return Error.Validation("Vehicle.InvalidImageUrl", "Image URL is required.");

        if (_images.Count >= 50)
            return Error.Validation("Vehicle.TooManyImages", "Maximum 50 images allowed.");

        if (isPrimary)
        {
            foreach (var img in _images)
                img.RemovePrimary();
        }

        var image = VehicleImage.Create(url, type, _images.Count, isPrimary || _images.Count == 0);
        _images.Add(image);
        UpdatedAt = DateTime.UtcNow;

        return Result.Success;
    }

    public ErrorOr<Success> MarkAsListed()
    {
        if (Status != VehicleStatus.Inspected)
            return Error.Conflict("Vehicle.MustBeInspected", "Vehicle must be inspected before listing.");

        if (ConditionReport is null)
            return Error.Conflict("Vehicle.NoConditionReport", "Vehicle must have a condition report.");

        Status = VehicleStatus.Listed;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success;
    }

    public ErrorOr<Success> MarkAsInAuction()
    {
        if (Status != VehicleStatus.Listed)
            return Error.Conflict("Vehicle.MustBeListed", "Vehicle must be listed before auction.");

        Status = VehicleStatus.InAuction;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success;
    }

    public void MarkAsSold()
    {
        Status = VehicleStatus.Sold;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsUnsold()
    {
        Status = VehicleStatus.Unsold;
        UpdatedAt = DateTime.UtcNow;
    }

    public ErrorOr<Success> FlagForMileageAnomaly(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            return Error.Validation("Vehicle.ReasonRequired", "Reason for mileage anomaly is required.");

        RaiseDomainEvent(new VehicleMileageAnomalyEvent(Id, reason));

        return Result.Success;
    }

    public void UpdateMileage(Mileage newMileage)
    {
        if (newMileage.ToMiles() < Mileage.ToMiles())
        {
            RaiseDomainEvent(new VehicleMileageAnomalyEvent(Id, "Mileage rollback detected"));
        }

        Mileage = newMileage;
        UpdatedAt = DateTime.UtcNow;
    }
}

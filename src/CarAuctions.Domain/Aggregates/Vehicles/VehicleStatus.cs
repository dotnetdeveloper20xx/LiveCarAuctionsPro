namespace CarAuctions.Domain.Aggregates.Vehicles;

public enum VehicleStatus
{
    Draft = 0,
    PendingInspection = 1,
    Inspected = 2,
    Listed = 3,
    InAuction = 4,
    Sold = 5,
    Unsold = 6,
    Released = 7,
    Archived = 8
}

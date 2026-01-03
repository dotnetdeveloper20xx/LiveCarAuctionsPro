using CarAuctions.Domain.Aggregates.Users;
using CarAuctions.Domain.Common;

namespace CarAuctions.Domain.Aggregates.Vehicles.Events;

public sealed class VehicleRegisteredEvent : DomainEventBase
{
    public VehicleId VehicleId { get; }
    public string VIN { get; }
    public UserId OwnerId { get; }

    public VehicleRegisteredEvent(VehicleId vehicleId, string vin, UserId ownerId)
    {
        VehicleId = vehicleId;
        VIN = vin;
        OwnerId = ownerId;
    }
}

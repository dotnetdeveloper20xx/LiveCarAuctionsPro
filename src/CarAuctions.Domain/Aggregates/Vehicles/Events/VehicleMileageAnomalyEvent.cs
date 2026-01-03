using CarAuctions.Domain.Common;

namespace CarAuctions.Domain.Aggregates.Vehicles.Events;

public sealed class VehicleMileageAnomalyEvent : DomainEventBase
{
    public VehicleId VehicleId { get; }
    public string Reason { get; }

    public VehicleMileageAnomalyEvent(VehicleId vehicleId, string reason)
    {
        VehicleId = vehicleId;
        Reason = reason;
    }
}

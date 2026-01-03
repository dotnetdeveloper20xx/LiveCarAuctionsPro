using CarAuctions.Domain.Aggregates.Users;
using CarAuctions.Domain.Common;

namespace CarAuctions.Domain.Aggregates.Vehicles.Events;

public sealed class VehicleInspectedEvent : DomainEventBase
{
    public VehicleId VehicleId { get; }
    public ConditionGrade OverallGrade { get; }
    public UserId InspectorId { get; }

    public VehicleInspectedEvent(VehicleId vehicleId, ConditionGrade overallGrade, UserId inspectorId)
    {
        VehicleId = vehicleId;
        OverallGrade = overallGrade;
        InspectorId = inspectorId;
    }
}

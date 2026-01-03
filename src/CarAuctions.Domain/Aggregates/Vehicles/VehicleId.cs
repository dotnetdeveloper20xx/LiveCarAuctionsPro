using CarAuctions.Domain.Common;

namespace CarAuctions.Domain.Aggregates.Vehicles;

public sealed class VehicleId : StronglyTypedId<VehicleId>
{
    public VehicleId(Guid value) : base(value)
    {
    }

    public static VehicleId CreateUnique() => new(Guid.NewGuid());

    public static VehicleId Create(Guid value) => new(value);
}

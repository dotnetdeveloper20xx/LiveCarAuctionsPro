using CarAuctions.Domain.Aggregates.Users;
using CarAuctions.Domain.Aggregates.Vehicles;
using CarAuctions.Domain.Common;

namespace CarAuctions.Domain.Aggregates.Auctions.Events;

public sealed class AuctionCreatedEvent : DomainEventBase
{
    public AuctionId AuctionId { get; }
    public VehicleId VehicleId { get; }
    public UserId SellerId { get; }
    public AuctionType Type { get; }
    public DateTime StartTime { get; }
    public DateTime EndTime { get; }

    public AuctionCreatedEvent(
        AuctionId auctionId,
        VehicleId vehicleId,
        UserId sellerId,
        AuctionType type,
        DateTime startTime,
        DateTime endTime)
    {
        AuctionId = auctionId;
        VehicleId = vehicleId;
        SellerId = sellerId;
        Type = type;
        StartTime = startTime;
        EndTime = endTime;
    }
}

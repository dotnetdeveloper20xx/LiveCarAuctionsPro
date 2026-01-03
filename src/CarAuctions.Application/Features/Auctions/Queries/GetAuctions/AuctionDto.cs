using CarAuctions.Domain.Aggregates.Auctions;

namespace CarAuctions.Application.Features.Auctions.Queries.GetAuctions;

public record AuctionDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public Guid VehicleId { get; init; }
    public Guid SellerId { get; init; }
    public decimal StartingPrice { get; init; }
    public string Currency { get; init; } = "USD";
    public decimal? ReservePrice { get; init; }
    public decimal? BuyNowPrice { get; init; }
    public decimal CurrentHighBid { get; init; }
    public DateTime StartTime { get; init; }
    public DateTime EndTime { get; init; }
    public DateTime? ActualEndTime { get; init; }
    public int BidCount { get; init; }
    public bool IsDealerOnly { get; init; }
    public string? Description { get; init; }
    public VehicleSummaryDto? Vehicle { get; init; }
}

public record VehicleSummaryDto
{
    public Guid Id { get; init; }
    public string VIN { get; init; } = string.Empty;
    public string Make { get; init; } = string.Empty;
    public string Model { get; init; } = string.Empty;
    public int Year { get; init; }
    public int Mileage { get; init; }
    public string? ExteriorColor { get; init; }
}

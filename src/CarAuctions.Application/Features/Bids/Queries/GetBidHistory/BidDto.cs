namespace CarAuctions.Application.Features.Bids.Queries.GetBidHistory;

public record BidDto
{
    public Guid Id { get; init; }
    public Guid AuctionId { get; init; }
    public Guid BidderId { get; init; }
    public string BidderName { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "USD";
    public string Status { get; init; } = string.Empty;
    public DateTime PlacedAt { get; init; }
    public bool IsProxyBid { get; init; }
    public decimal? MaxProxyAmount { get; init; }
}

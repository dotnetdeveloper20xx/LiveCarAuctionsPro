using CarAuctions.Domain.Aggregates.Auctions;
using CarAuctions.Domain.Aggregates.Bids.Events;
using CarAuctions.Domain.Aggregates.Users;
using CarAuctions.Domain.Common;
using ErrorOr;

namespace CarAuctions.Domain.Aggregates.Bids;

/// <summary>
/// Bid aggregate root.
/// </summary>
public sealed class Bid : AggregateRoot<BidId>
{
    public AuctionId AuctionId { get; private set; }
    public UserId BidderId { get; private set; }
    public Money Amount { get; private set; }
    public BidStatus Status { get; private set; }
    public DateTime PlacedAt { get; private set; }
    public bool IsProxyBid { get; private set; }
    public Money? MaxProxyAmount { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }

    private Bid(
        BidId id,
        AuctionId auctionId,
        UserId bidderId,
        Money amount,
        DateTime placedAt,
        bool isProxyBid,
        Money? maxProxyAmount,
        string? ipAddress,
        string? userAgent) : base(id)
    {
        AuctionId = auctionId;
        BidderId = bidderId;
        Amount = amount;
        PlacedAt = placedAt;
        IsProxyBid = isProxyBid;
        MaxProxyAmount = maxProxyAmount;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        Status = BidStatus.Active;
    }

    private Bid() : base()
    {
        AuctionId = null!;
        BidderId = null!;
        Amount = null!;
    }

    public static Bid Place(
        AuctionId auctionId,
        UserId bidderId,
        Money amount,
        DateTime placedAt,
        string? ipAddress = null,
        string? userAgent = null)
    {
        var bid = new Bid(
            BidId.CreateUnique(),
            auctionId,
            bidderId,
            amount,
            placedAt,
            isProxyBid: false,
            maxProxyAmount: null,
            ipAddress,
            userAgent);

        bid.RaiseDomainEvent(new BidPlacedEvent(bid.Id, auctionId, bidderId, amount));

        return bid;
    }

    public static ErrorOr<Bid> PlaceProxy(
        AuctionId auctionId,
        UserId bidderId,
        Money currentAmount,
        Money maxAmount,
        DateTime placedAt,
        string? ipAddress = null,
        string? userAgent = null)
    {
        if (maxAmount <= currentAmount)
            return Error.Validation("Bid.InvalidProxyMax", "Max proxy amount must exceed current amount.");

        var bid = new Bid(
            BidId.CreateUnique(),
            auctionId,
            bidderId,
            currentAmount,
            placedAt,
            isProxyBid: true,
            maxProxyAmount: maxAmount,
            ipAddress,
            userAgent);

        bid.RaiseDomainEvent(new ProxyBidPlacedEvent(bid.Id, auctionId, bidderId, currentAmount, maxAmount));

        return bid;
    }

    public ErrorOr<Success> IncrementProxyBid(Money newAmount)
    {
        if (!IsProxyBid || MaxProxyAmount is null)
            return Error.Conflict("Bid.NotProxy", "This is not a proxy bid.");

        if (newAmount > MaxProxyAmount)
            return Error.Validation("Bid.ExceedsMax", "Amount exceeds maximum proxy amount.");

        if (newAmount <= Amount)
            return Error.Validation("Bid.MustIncrease", "New amount must exceed current amount.");

        Amount = newAmount;

        return Result.Success;
    }

    public void MarkAsWinning()
    {
        Status = BidStatus.Winning;
        RaiseDomainEvent(new BidWonEvent(Id, AuctionId, BidderId, Amount));
    }

    public void MarkAsOutbid()
    {
        if (Status == BidStatus.Active)
        {
            Status = BidStatus.Outbid;
            RaiseDomainEvent(new BidOutbidEvent(Id, AuctionId, BidderId));
        }
    }

    public ErrorOr<Success> Withdraw(string reason)
    {
        if (Status != BidStatus.Active)
            return Error.Conflict("Bid.CannotWithdraw", "Only active bids can be withdrawn.");

        Status = BidStatus.Withdrawn;
        RaiseDomainEvent(new BidWithdrawnEvent(Id, AuctionId, BidderId, reason));

        return Result.Success;
    }

    public bool CanBeAutoBid(Money incomingBid)
    {
        if (!IsProxyBid || MaxProxyAmount is null || Status != BidStatus.Active)
            return false;

        return MaxProxyAmount > incomingBid;
    }
}

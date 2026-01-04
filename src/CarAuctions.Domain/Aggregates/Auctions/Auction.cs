using CarAuctions.Domain.Aggregates.Auctions.Events;
using CarAuctions.Domain.Aggregates.Bids;
using CarAuctions.Domain.Aggregates.Users;
using CarAuctions.Domain.Aggregates.Vehicles;
using CarAuctions.Domain.Common;
using CarAuctions.Domain.Exceptions;
using ErrorOr;

namespace CarAuctions.Domain.Aggregates.Auctions;

/// <summary>
/// Auction aggregate root.
/// </summary>
public sealed class Auction : AggregateRoot<AuctionId>
{
    private readonly List<BidId> _bidIds = new();
    private int _extensionCount = 0;

    public string Title { get; private set; }
    public string? Description { get; private set; }
    public AuctionType Type { get; private set; }
    public AuctionStatus Status { get; private set; }
    public VehicleId VehicleId { get; private set; }
    public UserId SellerId { get; private set; }
    public Money StartingPrice { get; private set; }
    public Money? ReservePrice { get; private set; }
    public Money? BuyNowPrice { get; private set; }
    public Money CurrentHighBid { get; private set; }
    public BidId? WinningBidId { get; private set; }
    public UserId? WinningBidderId { get; private set; }
    public AuctionSettings Settings { get; private set; }
    public DateTime StartTime { get; private set; }
    public DateTime EndTime { get; private set; }
    public DateTime? ActualEndTime { get; private set; }
    public bool IsDealerOnly { get; private set; }
    public int BidCount { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public IReadOnlyList<BidId> BidIds => _bidIds.AsReadOnly();

    private Auction(
        AuctionId id,
        string title,
        string? description,
        AuctionType type,
        VehicleId vehicleId,
        UserId sellerId,
        Money startingPrice,
        Money? reservePrice,
        Money? buyNowPrice,
        AuctionSettings settings,
        DateTime startTime,
        DateTime endTime,
        bool isDealerOnly) : base(id)
    {
        Title = title;
        Description = description;
        Type = type;
        VehicleId = vehicleId;
        SellerId = sellerId;
        StartingPrice = startingPrice;
        ReservePrice = reservePrice;
        BuyNowPrice = buyNowPrice;
        CurrentHighBid = Money.Zero(startingPrice.Currency);
        Settings = settings;
        StartTime = startTime;
        EndTime = endTime;
        IsDealerOnly = isDealerOnly;
        Status = AuctionStatus.Draft;
        BidCount = 0;
        CreatedAt = DateTime.UtcNow;
    }

    private Auction() : base()
    {
        Title = string.Empty;
        VehicleId = null!;
        SellerId = null!;
        StartingPrice = null!;
        CurrentHighBid = null!;
        Settings = null!;
    }

    public static ErrorOr<Auction> Create(
        string title,
        AuctionType type,
        VehicleId vehicleId,
        UserId sellerId,
        Money startingPrice,
        DateTime startTime,
        DateTime endTime,
        AuctionSettings? settings = null,
        Money? reservePrice = null,
        Money? buyNowPrice = null,
        string? description = null,
        bool isDealerOnly = false)
    {
        if (string.IsNullOrWhiteSpace(title))
            return Error.Validation("Auction.TitleRequired", "Title is required.");

        if (startTime >= endTime)
            return Error.Validation("Auction.InvalidTimes", "Start time must be before end time.");

        if (startTime < DateTime.UtcNow.AddMinutes(-5))
            return Error.Validation("Auction.StartTimeInPast", "Start time cannot be in the past.");

        if (reservePrice is not null && reservePrice < startingPrice)
            return Error.Validation("Auction.InvalidReserve", "Reserve price must be >= starting price.");

        if (buyNowPrice is not null && reservePrice is not null && buyNowPrice < reservePrice)
            return Error.Validation("Auction.InvalidBuyNow", "Buy now price must be >= reserve price.");

        var auction = new Auction(
            AuctionId.CreateUnique(),
            title.Trim(),
            description?.Trim(),
            type,
            vehicleId,
            sellerId,
            startingPrice,
            reservePrice,
            buyNowPrice,
            settings ?? AuctionSettings.Default(startingPrice.Currency),
            startTime,
            endTime,
            isDealerOnly);

        auction.RaiseDomainEvent(new AuctionCreatedEvent(
            auction.Id,
            auction.VehicleId,
            auction.SellerId,
            auction.Type,
            auction.StartTime,
            auction.EndTime));

        return auction;
    }

    /// <summary>
    /// Creates an auction for seeding/testing purposes, bypassing date validation.
    /// </summary>
    public static Auction CreateForSeeding(
        string title,
        AuctionType type,
        VehicleId vehicleId,
        UserId sellerId,
        Money startingPrice,
        DateTime startTime,
        DateTime endTime,
        AuctionSettings? settings = null,
        Money? reservePrice = null,
        Money? buyNowPrice = null,
        string? description = null,
        bool isDealerOnly = false)
    {
        return new Auction(
            AuctionId.CreateUnique(),
            title.Trim(),
            description?.Trim(),
            type,
            vehicleId,
            sellerId,
            startingPrice,
            reservePrice,
            buyNowPrice,
            settings ?? AuctionSettings.Default(startingPrice.Currency),
            startTime,
            endTime,
            isDealerOnly);
    }

    public ErrorOr<Success> Schedule()
    {
        if (Status != AuctionStatus.Draft)
            return Error.Conflict("Auction.NotDraft", "Auction must be in Draft status to schedule.");

        Status = AuctionStatus.Scheduled;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success;
    }

    public ErrorOr<Success> Start(DateTime currentTime)
    {
        if (Status != AuctionStatus.Scheduled)
            return Error.Conflict("Auction.NotScheduled", "Auction must be scheduled to start.");

        if (currentTime < StartTime)
            return Error.Conflict("Auction.NotStartTime", "Auction start time has not arrived.");

        Status = AuctionStatus.Active;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new AuctionStartedEvent(Id));

        return Result.Success;
    }

    public ErrorOr<Success> PlaceBid(BidId bidId, UserId bidderId, Money amount, DateTime bidTime)
    {
        if (Status != AuctionStatus.Active)
            return Error.Conflict("Auction.NotActive", "Auction is not active.");

        if (bidTime > EndTime)
            return Error.Conflict("Auction.Ended", "Auction has ended.");

        if (bidderId == SellerId)
            return Error.Validation("Auction.SellerCannotBid", "Seller cannot bid on their own auction.");

        var minimumBid = BidCount == 0
            ? StartingPrice
            : CurrentHighBid + Settings.MinimumBidIncrement;

        if (amount < minimumBid)
            return Error.Validation("Auction.BidTooLow", $"Bid must be at least {minimumBid}.");

        // Apply anti-sniping
        var timeToEnd = EndTime - bidTime;
        if (timeToEnd <= Settings.AntiSnipingWindow && _extensionCount < Settings.MaxExtensions)
        {
            EndTime = EndTime.Add(Settings.AntiSnipingExtension);
            _extensionCount++;
            RaiseDomainEvent(new AuctionExtendedEvent(Id, EndTime, _extensionCount));
        }

        _bidIds.Add(bidId);
        CurrentHighBid = amount;
        WinningBidId = bidId;
        WinningBidderId = bidderId;
        BidCount++;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new BidPlacedOnAuctionEvent(Id, bidId, bidderId, amount));

        return Result.Success;
    }

    public ErrorOr<Success> AcceptBuyNow(BidId bidId, UserId buyerId, DateTime purchaseTime)
    {
        if (Status != AuctionStatus.Active)
            return Error.Conflict("Auction.NotActive", "Auction is not active.");

        if (BuyNowPrice is null)
            return Error.Validation("Auction.NoBuyNow", "This auction does not have a Buy Now option.");

        if (buyerId == SellerId)
            return Error.Validation("Auction.SellerCannotBuy", "Seller cannot buy their own vehicle.");

        _bidIds.Add(bidId);
        CurrentHighBid = BuyNowPrice;
        WinningBidId = bidId;
        WinningBidderId = buyerId;
        BidCount++;
        Status = AuctionStatus.Completed;
        ActualEndTime = purchaseTime;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new AuctionCompletedEvent(Id, bidId, buyerId, BuyNowPrice, wasBuyNow: true));

        return Result.Success;
    }

    public ErrorOr<Success> Close(DateTime closeTime)
    {
        if (Status != AuctionStatus.Active)
            return Error.Conflict("Auction.NotActive", "Auction is not active.");

        if (closeTime < EndTime)
            return Error.Conflict("Auction.NotEnded", "Auction end time has not arrived.");

        ActualEndTime = closeTime;

        if (WinningBidId is not null && MeetsReserve())
        {
            Status = AuctionStatus.Completed;
            RaiseDomainEvent(new AuctionCompletedEvent(
                Id,
                WinningBidId,
                WinningBidderId!,
                CurrentHighBid,
                wasBuyNow: false));
        }
        else
        {
            Status = AuctionStatus.EndedNoSale;
            RaiseDomainEvent(new AuctionEndedNoSaleEvent(Id, CurrentHighBid, ReservePrice));
        }

        UpdatedAt = DateTime.UtcNow;

        return Result.Success;
    }

    public ErrorOr<Success> Cancel(string reason)
    {
        if (Status == AuctionStatus.Completed || Status == AuctionStatus.Cancelled)
            return Error.Conflict("Auction.CannotCancel", "Auction cannot be cancelled.");

        Status = AuctionStatus.Cancelled;
        ActualEndTime = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new AuctionCancelledEvent(Id, reason));

        return Result.Success;
    }

    public bool MeetsReserve() => ReservePrice is null || CurrentHighBid >= ReservePrice;

    public bool IsActive() => Status == AuctionStatus.Active;

    public bool HasEnded() => Status == AuctionStatus.Completed ||
                              Status == AuctionStatus.EndedNoSale ||
                              Status == AuctionStatus.Cancelled;

    public TimeSpan TimeRemaining(DateTime currentTime)
    {
        if (Status != AuctionStatus.Active)
            return TimeSpan.Zero;

        var remaining = EndTime - currentTime;
        return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
    }
}

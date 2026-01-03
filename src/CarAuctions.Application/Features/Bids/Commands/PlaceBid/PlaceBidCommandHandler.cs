using CarAuctions.Application.Common.Interfaces;
using CarAuctions.Domain.Aggregates.Auctions;
using CarAuctions.Domain.Aggregates.Bids;
using CarAuctions.Domain.Aggregates.Users;
using CarAuctions.Domain.Common;
using ErrorOr;
using MediatR;

namespace CarAuctions.Application.Features.Bids.Commands.PlaceBid;

public class PlaceBidCommandHandler : IRequestHandler<PlaceBidCommand, ErrorOr<Guid>>
{
    private readonly IAuctionRepository _auctionRepository;
    private readonly IBidRepository _bidRepository;
    private readonly IUserRepository _userRepository;
    private readonly IDateTime _dateTime;

    public PlaceBidCommandHandler(
        IAuctionRepository auctionRepository,
        IBidRepository bidRepository,
        IUserRepository userRepository,
        IDateTime dateTime)
    {
        _auctionRepository = auctionRepository;
        _bidRepository = bidRepository;
        _userRepository = userRepository;
        _dateTime = dateTime;
    }

    public async Task<ErrorOr<Guid>> Handle(
        PlaceBidCommand request,
        CancellationToken cancellationToken)
    {
        var auctionId = new AuctionId(request.AuctionId);
        var auction = await _auctionRepository.GetByIdAsync(auctionId, cancellationToken);

        if (auction is null)
        {
            return Error.NotFound("Auction.NotFound", "Auction not found");
        }

        if (auction.Status != AuctionStatus.Active)
        {
            return Error.Validation("Auction.NotActive", "Auction is not active");
        }

        var bidderId = new UserId(request.BidderId);
        var bidder = await _userRepository.GetByIdAsync(bidderId, cancellationToken);

        if (bidder is null)
        {
            return Error.NotFound("User.NotFound", "Bidder not found");
        }

        if (!bidder.Roles.HasFlag(UserRole.Buyer))
        {
            return Error.Validation("User.NotBuyer", "User is not registered as a buyer");
        }

        if (auction.IsDealerOnly && !bidder.IsDealer)
        {
            return Error.Validation("Auction.DealerOnly", "This auction is for dealers only");
        }

        var amount = Money.Create(request.Amount, request.Currency);

        if (amount.Currency != auction.StartingPrice.Currency)
        {
            return Error.Validation("Bid.CurrencyMismatch", "Bid currency must match auction currency");
        }

        var now = _dateTime.UtcNow;

        Bid bid;
        if (request.IsProxy && request.MaxProxyAmount.HasValue)
        {
            var bidResult = Bid.PlaceProxy(
                auctionId,
                bidderId,
                amount,
                Money.Create(request.MaxProxyAmount.Value, request.Currency),
                now);

            if (bidResult.IsError)
            {
                return bidResult.Errors;
            }
            bid = bidResult.Value;
        }
        else
        {
            bid = Bid.Place(auctionId, bidderId, amount, now);
        }

        var placeBidResult = auction.PlaceBid(bid.Id, bidderId, amount, now);
        if (placeBidResult.IsError)
        {
            return placeBidResult.Errors;
        }

        await _bidRepository.AddAsync(bid, cancellationToken);
        await _auctionRepository.UpdateAsync(auction, cancellationToken);

        return bid.Id.Value;
    }
}

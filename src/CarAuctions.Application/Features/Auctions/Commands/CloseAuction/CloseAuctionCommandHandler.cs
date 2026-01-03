using CarAuctions.Application.Common.Interfaces;
using CarAuctions.Domain.Aggregates.Auctions;
using ErrorOr;
using MediatR;

namespace CarAuctions.Application.Features.Auctions.Commands.CloseAuction;

public class CloseAuctionCommandHandler : IRequestHandler<CloseAuctionCommand, ErrorOr<Success>>
{
    private readonly IAuctionRepository _auctionRepository;
    private readonly IDateTime _dateTime;

    public CloseAuctionCommandHandler(
        IAuctionRepository auctionRepository,
        IDateTime dateTime)
    {
        _auctionRepository = auctionRepository;
        _dateTime = dateTime;
    }

    public async Task<ErrorOr<Success>> Handle(
        CloseAuctionCommand request,
        CancellationToken cancellationToken)
    {
        var auctionId = new AuctionId(request.AuctionId);
        var auction = await _auctionRepository.GetByIdAsync(auctionId, cancellationToken);

        if (auction is null)
        {
            return Error.NotFound("Auction.NotFound", "Auction not found");
        }

        var result = auction.Close(_dateTime.UtcNow);
        if (result.IsError)
        {
            return result.Errors;
        }

        await _auctionRepository.UpdateAsync(auction, cancellationToken);

        return Result.Success;
    }
}

using CarAuctions.Application.Common.Interfaces;
using CarAuctions.Domain.Aggregates.Auctions;
using ErrorOr;
using MediatR;

namespace CarAuctions.Application.Features.Auctions.Commands.StartAuction;

public class StartAuctionCommandHandler : IRequestHandler<StartAuctionCommand, ErrorOr<Success>>
{
    private readonly IAuctionRepository _auctionRepository;
    private readonly IDateTime _dateTime;

    public StartAuctionCommandHandler(
        IAuctionRepository auctionRepository,
        IDateTime dateTime)
    {
        _auctionRepository = auctionRepository;
        _dateTime = dateTime;
    }

    public async Task<ErrorOr<Success>> Handle(
        StartAuctionCommand request,
        CancellationToken cancellationToken)
    {
        var auctionId = new AuctionId(request.AuctionId);
        var auction = await _auctionRepository.GetByIdAsync(auctionId, cancellationToken);

        if (auction is null)
        {
            return Error.NotFound("Auction.NotFound", "Auction not found");
        }

        var result = auction.Start(_dateTime.UtcNow);
        if (result.IsError)
        {
            return result.Errors;
        }

        await _auctionRepository.UpdateAsync(auction, cancellationToken);

        return Result.Success;
    }
}

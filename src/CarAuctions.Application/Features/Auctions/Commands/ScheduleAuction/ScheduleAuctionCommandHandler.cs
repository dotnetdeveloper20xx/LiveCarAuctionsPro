using CarAuctions.Application.Common.Interfaces;
using CarAuctions.Domain.Aggregates.Auctions;
using ErrorOr;
using MediatR;

namespace CarAuctions.Application.Features.Auctions.Commands.ScheduleAuction;

public class ScheduleAuctionCommandHandler : IRequestHandler<ScheduleAuctionCommand, ErrorOr<Success>>
{
    private readonly IAuctionRepository _auctionRepository;

    public ScheduleAuctionCommandHandler(IAuctionRepository auctionRepository)
    {
        _auctionRepository = auctionRepository;
    }

    public async Task<ErrorOr<Success>> Handle(
        ScheduleAuctionCommand request,
        CancellationToken cancellationToken)
    {
        var auctionId = new AuctionId(request.AuctionId);
        var auction = await _auctionRepository.GetByIdAsync(auctionId, cancellationToken);

        if (auction is null)
        {
            return Error.NotFound("Auction.NotFound", "Auction not found");
        }

        var result = auction.Schedule();
        if (result.IsError)
        {
            return result.Errors;
        }

        await _auctionRepository.UpdateAsync(auction, cancellationToken);
        await _auctionRepository.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }
}

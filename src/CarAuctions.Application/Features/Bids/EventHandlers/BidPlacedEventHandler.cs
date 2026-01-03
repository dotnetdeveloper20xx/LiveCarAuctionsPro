using CarAuctions.Application.Common.Interfaces;
using CarAuctions.Domain.Aggregates.Bids.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CarAuctions.Application.Features.Bids.EventHandlers;

public class BidPlacedEventHandler : INotificationHandler<BidPlacedEvent>
{
    private readonly ILogger<BidPlacedEventHandler> _logger;

    public BidPlacedEventHandler(ILogger<BidPlacedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(BidPlacedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Bid {BidId} placed on auction {AuctionId} by user {BidderId} for {Amount}",
            notification.BidId.Value,
            notification.AuctionId.Value,
            notification.BidderId.Value,
            notification.Amount);

        return Task.CompletedTask;
    }
}

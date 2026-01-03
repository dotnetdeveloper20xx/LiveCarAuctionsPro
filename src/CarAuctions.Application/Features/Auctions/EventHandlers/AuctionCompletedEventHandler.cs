using CarAuctions.Application.Common.Interfaces;
using CarAuctions.Domain.Aggregates.Auctions.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CarAuctions.Application.Features.Auctions.EventHandlers;

public class AuctionCompletedEventHandler : INotificationHandler<AuctionCompletedEvent>
{
    private readonly ILogger<AuctionCompletedEventHandler> _logger;
    private readonly IEmailService _emailService;

    public AuctionCompletedEventHandler(
        ILogger<AuctionCompletedEventHandler> logger,
        IEmailService emailService)
    {
        _logger = logger;
        _emailService = emailService;
    }

    public async Task Handle(AuctionCompletedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Auction {AuctionId} completed. Winner: {WinnerId}, Final Price: {FinalPrice}, BuyNow: {WasBuyNow}",
            notification.AuctionId.Value,
            notification.WinnerId.Value,
            notification.FinalPrice,
            notification.WasBuyNow);

        // Send winner notification
        await _emailService.SendAuctionWonNotificationAsync(
            notification.WinnerId.Value,
            notification.AuctionId.Value,
            notification.FinalPrice.Amount,
            cancellationToken);
    }
}

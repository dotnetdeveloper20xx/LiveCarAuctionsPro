using CarAuctions.Application.Common.Interfaces;
using CarAuctions.Domain.Aggregates.Bids.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CarAuctions.Application.Features.Bids.EventHandlers;

public class BidOutbidEventHandler : INotificationHandler<BidOutbidEvent>
{
    private readonly ILogger<BidOutbidEventHandler> _logger;
    private readonly IEmailService _emailService;

    public BidOutbidEventHandler(
        ILogger<BidOutbidEventHandler> logger,
        IEmailService emailService)
    {
        _logger = logger;
        _emailService = emailService;
    }

    public async Task Handle(BidOutbidEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Bid {BidId} on auction {AuctionId} was outbid. Notifying user {BidderId}",
            notification.BidId.Value,
            notification.AuctionId.Value,
            notification.BidderId.Value);

        // Send outbid notification email
        await _emailService.SendOutbidNotificationAsync(
            notification.BidderId.Value,
            notification.AuctionId.Value,
            cancellationToken);
    }
}

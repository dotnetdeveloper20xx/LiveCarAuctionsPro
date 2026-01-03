using CarAuctions.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace CarAuctions.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    public Task SendOutbidNotificationAsync(
        Guid userId,
        Guid auctionId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Sending outbid notification to user {UserId} for auction {AuctionId}",
            userId, auctionId);

        // In production, integrate with actual email service (SendGrid, SES, etc.)
        return Task.CompletedTask;
    }

    public Task SendAuctionWonNotificationAsync(
        Guid userId,
        Guid auctionId,
        decimal finalPrice,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Sending auction won notification to user {UserId} for auction {AuctionId} at {FinalPrice}",
            userId, auctionId, finalPrice);

        return Task.CompletedTask;
    }

    public Task SendAuctionEndingSoonNotificationAsync(
        Guid userId,
        Guid auctionId,
        TimeSpan timeRemaining,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Sending auction ending soon notification to user {UserId} for auction {AuctionId}. Time remaining: {TimeRemaining}",
            userId, auctionId, timeRemaining);

        return Task.CompletedTask;
    }

    public Task SendEmailAsync(
        string to,
        string subject,
        string body,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Sending email to {To} with subject: {Subject}",
            to, subject);

        return Task.CompletedTask;
    }
}

namespace CarAuctions.Application.Common.Interfaces;

public interface IEmailService
{
    Task SendOutbidNotificationAsync(
        Guid userId,
        Guid auctionId,
        CancellationToken cancellationToken = default);

    Task SendAuctionWonNotificationAsync(
        Guid userId,
        Guid auctionId,
        decimal finalPrice,
        CancellationToken cancellationToken = default);

    Task SendAuctionEndingSoonNotificationAsync(
        Guid userId,
        Guid auctionId,
        TimeSpan timeRemaining,
        CancellationToken cancellationToken = default);

    Task SendEmailAsync(
        string to,
        string subject,
        string body,
        CancellationToken cancellationToken = default);
}

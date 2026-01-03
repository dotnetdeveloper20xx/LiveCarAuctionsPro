using CarAuctions.Application.Common.Interfaces;
using CarAuctions.Application.Features.Auctions.Commands.CloseAuction;
using CarAuctions.Domain.Aggregates.Auctions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CarAuctions.Worker.Services;

public class AuctionTimerService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AuctionTimerService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(30);

    public AuctionTimerService(
        IServiceProvider serviceProvider,
        ILogger<AuctionTimerService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Auction Timer Service starting...");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessEndedAuctionsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing ended auctions");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("Auction Timer Service stopping...");
    }

    private async Task ProcessEndedAuctionsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        var mediator = scope.ServiceProvider.GetRequiredService<ISender>();
        var dateTime = scope.ServiceProvider.GetRequiredService<IDateTime>();

        var now = dateTime.UtcNow;

        // Find active auctions that have passed their end time
        var endedAuctions = await context.Auctions
            .Where(a => a.Status == AuctionStatus.Active && a.EndTime <= now)
            .Select(a => a.Id.Value)
            .ToListAsync(cancellationToken);

        if (endedAuctions.Count > 0)
        {
            _logger.LogInformation("Found {Count} auctions to close", endedAuctions.Count);

            foreach (var auctionId in endedAuctions)
            {
                try
                {
                    var command = new CloseAuctionCommand(auctionId);
                    var result = await mediator.Send(command, cancellationToken);

                    if (result.IsError)
                    {
                        _logger.LogWarning(
                            "Failed to close auction {AuctionId}: {Error}",
                            auctionId,
                            result.FirstError.Description);
                    }
                    else
                    {
                        _logger.LogInformation("Closed auction {AuctionId}", auctionId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error closing auction {AuctionId}", auctionId);
                }
            }
        }
    }
}

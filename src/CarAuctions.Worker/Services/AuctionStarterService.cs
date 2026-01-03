using CarAuctions.Application.Common.Interfaces;
using CarAuctions.Application.Features.Auctions.Commands.StartAuction;
using CarAuctions.Domain.Aggregates.Auctions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CarAuctions.Worker.Services;

public class AuctionStarterService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AuctionStarterService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(30);

    public AuctionStarterService(
        IServiceProvider serviceProvider,
        ILogger<AuctionStarterService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Auction Starter Service starting...");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await StartScheduledAuctionsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting scheduled auctions");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("Auction Starter Service stopping...");
    }

    private async Task StartScheduledAuctionsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        var mediator = scope.ServiceProvider.GetRequiredService<ISender>();
        var dateTime = scope.ServiceProvider.GetRequiredService<IDateTime>();

        var now = dateTime.UtcNow;

        // Find scheduled auctions that should start
        var auctionsToStart = await context.Auctions
            .Where(a => a.Status == AuctionStatus.Scheduled && a.StartTime <= now)
            .Select(a => a.Id.Value)
            .ToListAsync(cancellationToken);

        if (auctionsToStart.Count > 0)
        {
            _logger.LogInformation("Found {Count} auctions to start", auctionsToStart.Count);

            foreach (var auctionId in auctionsToStart)
            {
                try
                {
                    var command = new StartAuctionCommand(auctionId);
                    var result = await mediator.Send(command, cancellationToken);

                    if (result.IsError)
                    {
                        _logger.LogWarning(
                            "Failed to start auction {AuctionId}: {Error}",
                            auctionId,
                            result.FirstError.Description);
                    }
                    else
                    {
                        _logger.LogInformation("Started auction {AuctionId}", auctionId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error starting auction {AuctionId}", auctionId);
                }
            }
        }
    }
}

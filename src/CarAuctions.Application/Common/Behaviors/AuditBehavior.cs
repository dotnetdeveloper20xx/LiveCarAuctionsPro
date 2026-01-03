using CarAuctions.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CarAuctions.Application.Common.Behaviors;

public class AuditBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<AuditBehavior<TRequest, TResponse>> _logger;

    public AuditBehavior(
        IAuditService auditService,
        ICurrentUserService currentUser,
        ILogger<AuditBehavior<TRequest, TResponse>> logger)
    {
        _auditService = auditService;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        // Only audit commands (not queries)
        if (!requestName.EndsWith("Command"))
        {
            return await next();
        }

        var response = await next();

        try
        {
            // Extract entity info from command name (e.g., CreateAuctionCommand -> Auction)
            var entityType = ExtractEntityType(requestName);
            var entityId = ExtractEntityId(request);

            Guid? userId = Guid.TryParse(_currentUser.UserId, out var parsedUserId)
                ? parsedUserId
                : null;

            var entry = new AuditEntry(
                Id: Guid.NewGuid(),
                EntityType: entityType,
                EntityId: entityId,
                Action: requestName.Replace("Command", ""),
                UserId: userId,
                UserEmail: _currentUser.Email,
                OldValues: null,
                NewValues: SerializeRequest(request),
                IpAddress: null,
                Timestamp: DateTime.UtcNow
            );

            await _auditService.LogAsync(entry, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to create audit log for {RequestName}", requestName);
        }

        return response;
    }

    private static string ExtractEntityType(string requestName)
    {
        // CreateAuctionCommand -> Auction
        // PlaceBidCommand -> Bid
        var withoutCommand = requestName.Replace("Command", "");
        var actions = new[] { "Create", "Update", "Delete", "Place", "Start", "Close", "Cancel", "Initiate", "Process" };

        foreach (var action in actions)
        {
            if (withoutCommand.StartsWith(action))
            {
                return withoutCommand.Substring(action.Length);
            }
        }

        return withoutCommand;
    }

    private static Guid ExtractEntityId(TRequest request)
    {
        // Try to get Id, AuctionId, or similar property
        var type = typeof(TRequest);
        var idProps = new[] { "Id", "AuctionId", "VehicleId", "UserId", "PaymentId", "BidId" };

        foreach (var propName in idProps)
        {
            var prop = type.GetProperty(propName);
            if (prop != null && prop.PropertyType == typeof(Guid))
            {
                var value = prop.GetValue(request);
                if (value is Guid guidValue)
                {
                    return guidValue;
                }
            }
        }

        return Guid.Empty;
    }

    private static string? SerializeRequest(TRequest request)
    {
        try
        {
            return JsonSerializer.Serialize(request, new JsonSerializerOptions
            {
                WriteIndented = false,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            });
        }
        catch
        {
            return null;
        }
    }
}

using CarAuctions.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace CarAuctions.Infrastructure.Services;

public class AuditService : IAuditService
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<AuditService> _logger;
    private readonly List<AuditEntry> _auditLogs = new(); // In production, use database

    public AuditService(
        IApplicationDbContext context,
        ILogger<AuditService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public Task LogAsync(AuditEntry entry, CancellationToken cancellationToken = default)
    {
        _auditLogs.Add(entry);

        _logger.LogInformation(
            "Audit: {Action} on {EntityType} {EntityId} by User {UserId}",
            entry.Action,
            entry.EntityType,
            entry.EntityId,
            entry.UserId);

        // In production, save to AuditLogs table
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<AuditEntry>> GetAuditLogsAsync(
        string? entityType = null,
        Guid? entityId = null,
        Guid? userId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int pageNumber = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var query = _auditLogs.AsEnumerable();

        if (!string.IsNullOrEmpty(entityType))
            query = query.Where(a => a.EntityType == entityType);

        if (entityId.HasValue)
            query = query.Where(a => a.EntityId == entityId.Value);

        if (userId.HasValue)
            query = query.Where(a => a.UserId == userId.Value);

        if (fromDate.HasValue)
            query = query.Where(a => a.Timestamp >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(a => a.Timestamp <= toDate.Value);

        var result = query
            .OrderByDescending(a => a.Timestamp)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Task.FromResult<IReadOnlyList<AuditEntry>>(result);
    }
}

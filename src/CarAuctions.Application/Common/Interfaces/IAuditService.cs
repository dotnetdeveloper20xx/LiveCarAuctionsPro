namespace CarAuctions.Application.Common.Interfaces;

public interface IAuditService
{
    Task LogAsync(AuditEntry entry, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AuditEntry>> GetAuditLogsAsync(
        string? entityType = null,
        Guid? entityId = null,
        Guid? userId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int pageNumber = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default);
}

public record AuditEntry(
    Guid Id,
    string EntityType,
    Guid EntityId,
    string Action,
    Guid? UserId,
    string? UserEmail,
    string? OldValues,
    string? NewValues,
    string? IpAddress,
    DateTime Timestamp
);

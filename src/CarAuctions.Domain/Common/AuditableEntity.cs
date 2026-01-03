namespace CarAuctions.Domain.Common;

/// <summary>
/// Base class for entities that require audit information.
/// </summary>
/// <typeparam name="TId">The type of the entity identifier.</typeparam>
public abstract class AuditableEntity<TId> : BaseEntity<TId>
    where TId : notnull
{
    protected AuditableEntity(TId id) : base(id)
    {
    }

    /// <summary>
    /// For EF Core.
    /// </summary>
    protected AuditableEntity() : base()
    {
    }

    /// <summary>
    /// Gets or sets the date and time when the entity was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who created the entity.
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the entity was last modified.
    /// </summary>
    public DateTime? LastModifiedAt { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who last modified the entity.
    /// </summary>
    public string? LastModifiedBy { get; set; }
}

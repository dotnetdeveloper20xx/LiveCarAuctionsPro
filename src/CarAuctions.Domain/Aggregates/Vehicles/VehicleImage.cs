using CarAuctions.Domain.Common;

namespace CarAuctions.Domain.Aggregates.Vehicles;

/// <summary>
/// Entity representing a vehicle image.
/// </summary>
public sealed class VehicleImage : BaseEntity<Guid>
{
    public string Url { get; private set; }
    public ImageType Type { get; private set; }
    public int DisplayOrder { get; private set; }
    public bool IsPrimary { get; private set; }
    public DateTime UploadedAt { get; private set; }

    private VehicleImage(Guid id, string url, ImageType type, int displayOrder, bool isPrimary, DateTime uploadedAt)
        : base(id)
    {
        Url = url;
        Type = type;
        DisplayOrder = displayOrder;
        IsPrimary = isPrimary;
        UploadedAt = uploadedAt;
    }

    private VehicleImage() : base()
    {
        Url = string.Empty;
    }

    public static VehicleImage Create(string url, ImageType type, int displayOrder = 0, bool isPrimary = false)
    {
        return new VehicleImage(
            Guid.NewGuid(),
            url,
            type,
            displayOrder,
            isPrimary,
            DateTime.UtcNow);
    }

    public void SetAsPrimary()
    {
        IsPrimary = true;
    }

    public void RemovePrimary()
    {
        IsPrimary = false;
    }
}

public enum ImageType
{
    Exterior = 0,
    Interior = 1,
    Engine = 2,
    Undercarriage = 3,
    Damage = 4,
    Documents = 5,
    Other = 6
}

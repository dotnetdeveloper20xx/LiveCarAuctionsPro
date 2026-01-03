using CarAuctions.Application.Common.Interfaces;

namespace CarAuctions.Infrastructure.Services;

public sealed class DateTimeService : IDateTime
{
    public DateTime UtcNow => DateTime.UtcNow;
}

using CarAuctions.Application.Common.Interfaces;
using CarAuctions.Domain.Aggregates.Users;
using CarAuctions.Domain.Aggregates.Vehicles;
using Microsoft.EntityFrameworkCore;

namespace CarAuctions.Persistence.Repositories;

public class VehicleRepository : Repository<Vehicle, VehicleId>, IVehicleRepository
{
    public VehicleRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Vehicle?> GetByVinAsync(string vin, CancellationToken cancellationToken = default)
    {
        var normalizedVin = vin.ToUpperInvariant().Trim();
        return await DbSet
            .FirstOrDefaultAsync(v => v.VIN.Value == normalizedVin, cancellationToken);
    }

    public async Task<IReadOnlyList<Vehicle>> GetByOwnerAsync(
        UserId ownerId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(v => v.OwnerId == ownerId)
            .OrderByDescending(v => v.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Vehicle>> GetByStatusAsync(
        VehicleStatus status,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(v => v.Status == status)
            .OrderByDescending(v => v.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Vehicle>> SearchAsync(
        string? make,
        string? model,
        int? yearFrom,
        int? yearTo,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsQueryable();

        if (!string.IsNullOrWhiteSpace(make))
            query = query.Where(v => v.Make.Contains(make));

        if (!string.IsNullOrWhiteSpace(model))
            query = query.Where(v => v.Model.Contains(model));

        if (yearFrom.HasValue)
            query = query.Where(v => v.Year >= yearFrom.Value);

        if (yearTo.HasValue)
            query = query.Where(v => v.Year <= yearTo.Value);

        return await query.OrderByDescending(v => v.Year).ToListAsync(cancellationToken);
    }

    public async Task<bool> VinExistsAsync(string vin, CancellationToken cancellationToken = default)
    {
        var normalizedVin = vin.ToUpperInvariant().Trim();
        return await DbSet.AnyAsync(v => v.VIN.Value == normalizedVin, cancellationToken);
    }
}

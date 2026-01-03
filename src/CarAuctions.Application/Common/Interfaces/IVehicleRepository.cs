using CarAuctions.Domain.Aggregates.Users;
using CarAuctions.Domain.Aggregates.Vehicles;

namespace CarAuctions.Application.Common.Interfaces;

public interface IVehicleRepository : IRepository<Vehicle, VehicleId>
{
    Task<Vehicle?> GetByVinAsync(string vin, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Vehicle>> GetByOwnerAsync(UserId ownerId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Vehicle>> GetByStatusAsync(VehicleStatus status, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Vehicle>> SearchAsync(string? make, string? model, int? yearFrom, int? yearTo, CancellationToken cancellationToken = default);
    Task<bool> VinExistsAsync(string vin, CancellationToken cancellationToken = default);
}

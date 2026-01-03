using CarAuctions.Domain.Aggregates.Users;

namespace CarAuctions.Application.Common.Interfaces;

public interface IUserRepository : IRepository<User, UserId>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<User>> GetByRoleAsync(UserRole role, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<User>> GetDealersAsync(CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
}

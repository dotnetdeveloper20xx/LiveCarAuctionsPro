using CarAuctions.Application.Common.Interfaces;
using CarAuctions.Domain.Aggregates.Users;
using Microsoft.EntityFrameworkCore;

namespace CarAuctions.Persistence.Repositories;

public class UserRepository : Repository<User, UserId>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.ToLowerInvariant().Trim();
        return await DbSet
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);
    }

    public async Task<IReadOnlyList<User>> GetByRoleAsync(
        UserRole role,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(u => (u.Roles & role) == role)
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<User>> GetDealersAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(u => u.IsDealer)
            .OrderBy(u => u.CompanyName)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.ToLowerInvariant().Trim();
        return await DbSet.AnyAsync(u => u.Email == normalizedEmail, cancellationToken);
    }
}

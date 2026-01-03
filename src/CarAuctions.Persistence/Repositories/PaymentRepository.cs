using CarAuctions.Application.Common.Interfaces;
using CarAuctions.Domain.Aggregates.Auctions;
using CarAuctions.Domain.Aggregates.Payments;
using CarAuctions.Domain.Aggregates.Users;
using Microsoft.EntityFrameworkCore;

namespace CarAuctions.Persistence.Repositories;

public class PaymentRepository : Repository<Payment, PaymentId>, IPaymentRepository
{
    public PaymentRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Payment?> GetByAuctionIdAsync(
        AuctionId auctionId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(p => p.Invoices)
            .FirstOrDefaultAsync(p => p.AuctionId == auctionId, cancellationToken);
    }

    public async Task<IReadOnlyList<Payment>> GetByUserIdAsync(
        UserId userId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(p => p.UserId == userId)
            .Include(p => p.Invoices)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Payment>> GetPendingPaymentsAsync(
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(p => p.Status == PaymentStatus.Pending || p.Status == PaymentStatus.Processing)
            .OrderBy(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}

using CarAuctions.Domain.Aggregates.Auctions;
using CarAuctions.Domain.Aggregates.Payments;
using CarAuctions.Domain.Aggregates.Users;

namespace CarAuctions.Application.Common.Interfaces;

public interface IPaymentRepository : IRepository<Payment, PaymentId>
{
    Task<Payment?> GetByAuctionIdAsync(AuctionId auctionId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Payment>> GetByUserIdAsync(UserId userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Payment>> GetPendingPaymentsAsync(CancellationToken cancellationToken = default);
}

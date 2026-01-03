using CarAuctions.Application.Common.Interfaces;
using CarAuctions.Domain.Aggregates.Users;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CarAuctions.Application.Features.Users.Queries.GetUserById;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, ErrorOr<UserDto>>
{
    private readonly IApplicationDbContext _context;

    public GetUserByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<UserDto>> Handle(
        GetUserByIdQuery request,
        CancellationToken cancellationToken)
    {
        var userId = new UserId(request.Id);

        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null)
        {
            return Error.NotFound("User.NotFound", "User not found");
        }

        var roles = Enum.GetValues<UserRole>()
            .Where(r => r != UserRole.None && user.Roles.HasFlag(r))
            .Select(r => r.ToString())
            .ToArray();

        return new UserDto
        {
            Id = user.Id.Value,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FullName = user.FullName,
            PhoneNumber = user.Phone,
            Status = user.Status.ToString(),
            Roles = roles,
            IsDealer = user.IsDealer,
            DealerLicenseNumber = user.DealerLicenseNumber,
            DealerCompanyName = user.CompanyName,
            CreditLimit = user.CreditLimit?.TotalLimit.Amount,
            CreditLimitCurrency = user.CreditLimit?.TotalLimit.Currency,
            CreatedAt = user.CreatedAt
        };
    }
}

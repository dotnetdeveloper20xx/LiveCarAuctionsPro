using CarAuctions.Application.Common.Interfaces;
using CarAuctions.Domain.Aggregates.Users;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CarAuctions.Application.Features.Users.Commands.RegisterUser;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, ErrorOr<Guid>>
{
    private readonly IUserRepository _userRepository;
    private readonly IApplicationDbContext _context;

    public RegisterUserCommandHandler(
        IUserRepository userRepository,
        IApplicationDbContext context)
    {
        _userRepository = userRepository;
        _context = context;
    }

    public async Task<ErrorOr<Guid>> Handle(
        RegisterUserCommand request,
        CancellationToken cancellationToken)
    {
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (existingUser is not null)
        {
            return Error.Conflict("User.EmailExists", "A user with this email already exists");
        }

        var roles = UserRole.None;
        foreach (var role in request.Roles)
        {
            if (Enum.TryParse<UserRole>(role, true, out var parsedRole))
            {
                roles |= parsedRole;
            }
        }

        var userResult = User.Create(
            request.Email,
            request.FirstName,
            request.LastName,
            roles,
            request.PhoneNumber);

        if (userResult.IsError)
        {
            return userResult.Errors;
        }

        var user = userResult.Value;
        await _userRepository.AddAsync(user, cancellationToken);

        return user.Id.Value;
    }
}

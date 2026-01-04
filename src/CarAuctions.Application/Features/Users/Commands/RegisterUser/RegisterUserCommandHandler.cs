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
    private readonly IPasswordHasher _passwordHasher;

    public RegisterUserCommandHandler(
        IUserRepository userRepository,
        IApplicationDbContext context,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<ErrorOr<Guid>> Handle(
        RegisterUserCommand request,
        CancellationToken cancellationToken)
    {
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email.ToLowerInvariant(), cancellationToken);

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

        var passwordHash = _passwordHasher.HashPassword(request.Password);

        var userResult = User.Create(
            request.Email,
            passwordHash,
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
        await _userRepository.SaveChangesAsync(cancellationToken);

        return user.Id.Value;
    }
}

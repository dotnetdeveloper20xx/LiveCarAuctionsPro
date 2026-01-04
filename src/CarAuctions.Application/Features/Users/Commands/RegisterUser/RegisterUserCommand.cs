using ErrorOr;
using MediatR;

namespace CarAuctions.Application.Features.Users.Commands.RegisterUser;

public record RegisterUserCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string[] Roles,
    string? PhoneNumber = null) : IRequest<ErrorOr<Guid>>;

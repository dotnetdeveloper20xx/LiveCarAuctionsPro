using ErrorOr;
using MediatR;

namespace CarAuctions.Application.Features.Users.Queries.GetUserById;

public record GetUserByIdQuery(Guid Id) : IRequest<ErrorOr<UserDto>>;

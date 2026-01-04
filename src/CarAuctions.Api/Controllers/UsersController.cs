using CarAuctions.Application.Common.Interfaces;
using CarAuctions.Application.Features.Users.Commands.RegisterUser;
using CarAuctions.Application.Features.Users.Queries.GetUserById;
using CarAuctions.Domain.Aggregates.Users;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CarAuctions.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly ISender _mediator;
    private readonly IUserRepository _userRepository;

    public UsersController(ISender mediator, IUserRepository userRepository)
    {
        _mediator = mediator;
        _userRepository = userRepository;
    }

    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(CurrentUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var user = await _userRepository.GetByIdAsync(new UserId(userId), cancellationToken);

        if (user is null)
        {
            return Unauthorized();
        }

        var roles = new List<string>();
        foreach (var role in Enum.GetValues<UserRole>())
        {
            if (role != UserRole.None && user.Roles.HasFlag(role))
            {
                roles.Add(role.ToString());
            }
        }

        return Ok(new CurrentUserDto(
            user.Id.Value,
            user.Email,
            user.FirstName,
            user.LastName,
            roles.ToArray(),
            user.IsDealer,
            user.CreditLimit?.TotalLimit.Amount));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUser(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetUserByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);

        return result.Match(
            success => Ok(success),
            errors => Problem(errors));
    }

    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegisterUser(
        [FromBody] RegisterUserCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);

        return result.Match(
            id => CreatedAtAction(nameof(GetUser), new { id }, id),
            errors => Problem(errors));
    }

    private IActionResult Problem(List<ErrorOr.Error> errors)
    {
        var firstError = errors.First();

        var statusCode = firstError.Type switch
        {
            ErrorOr.ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorOr.ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorOr.ErrorType.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };

        return Problem(
            statusCode: statusCode,
            title: firstError.Code,
            detail: firstError.Description);
    }
}

public record CurrentUserDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string[] Roles,
    bool IsDealer,
    decimal? CreditLimit);

using CarAuctions.Application.Common.Interfaces;
using CarAuctions.Application.Features.Users.Commands.RegisterUser;
using CarAuctions.Domain.Aggregates.Users;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarAuctions.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ISender _mediator;
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        ISender mediator,
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService,
        ILogger<AuthController> logger)
    {
        _mediator = mediator;
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);

        if (user is null)
        {
            _logger.LogWarning("Login failed: user not found for email {Email}", request.Email);
            return Unauthorized(new { error = "Invalid email or password" });
        }

        if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Login failed: invalid password for email {Email}", request.Email);
            return Unauthorized(new { error = "Invalid email or password" });
        }

        if (user.Status == UserStatus.Suspended)
        {
            _logger.LogWarning("Login failed: user {Email} is suspended", request.Email);
            return Unauthorized(new { error = "Account is suspended" });
        }

        user.RecordLogin();
        await _userRepository.UpdateAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        var accessToken = _jwtTokenService.GenerateAccessToken(user);
        var refreshToken = _jwtTokenService.GenerateRefreshToken();

        _logger.LogInformation("User {Email} logged in successfully", request.Email);

        return Ok(new AuthResponse(
            accessToken,
            refreshToken,
            DateTime.UtcNow.AddMinutes(60),
            MapToUserDto(user)));
    }

    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var roles = new List<string> { "Buyer" };
        if (request.IsDealer)
        {
            roles.Add("Dealer");
            roles.Add("Seller");
        }

        var command = new RegisterUserCommand(
            request.Email,
            request.Password,
            request.FirstName,
            request.LastName,
            roles.ToArray(),
            request.Phone);

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsError)
        {
            var firstError = result.FirstError;
            return firstError.Type switch
            {
                ErrorOr.ErrorType.Conflict => Conflict(new { error = firstError.Description }),
                ErrorOr.ErrorType.Validation => BadRequest(new { error = firstError.Description }),
                _ => BadRequest(new { error = firstError.Description })
            };
        }

        // Get the created user and generate tokens
        var user = await _userRepository.GetByIdAsync(new UserId(result.Value), cancellationToken);
        if (user is null)
        {
            return BadRequest(new { error = "Failed to create user" });
        }

        // Auto-activate the user for now (in production, you might want email verification)
        user.Activate();
        await _userRepository.UpdateAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        var accessToken = _jwtTokenService.GenerateAccessToken(user);
        var refreshToken = _jwtTokenService.GenerateRefreshToken();

        _logger.LogInformation("User {Email} registered successfully", request.Email);

        return Ok(new AuthResponse(
            accessToken,
            refreshToken,
            DateTime.UtcNow.AddMinutes(60),
            MapToUserDto(user)));
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken(
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        if (!_jwtTokenService.ValidateRefreshToken(request.RefreshToken))
        {
            return Unauthorized(new { error = "Invalid refresh token" });
        }

        // In production, you would look up the refresh token in the database
        // and get the associated user. For now, this is a simplified implementation.
        return Unauthorized(new { error = "Refresh token expired. Please login again." });
    }

    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var user = await _userRepository.GetByIdAsync(new UserId(userId), cancellationToken);

        if (user is null)
        {
            return Unauthorized();
        }

        return Ok(MapToUserDto(user));
    }

    private static UserDto MapToUserDto(User user)
    {
        var roles = new List<string>();
        foreach (var role in Enum.GetValues<UserRole>())
        {
            if (role != UserRole.None && user.Roles.HasFlag(role))
            {
                roles.Add(role.ToString());
            }
        }

        return new UserDto(
            user.Id.Value,
            user.Email,
            user.FirstName,
            user.LastName,
            roles.ToArray(),
            user.IsDealer,
            user.CreditLimit?.TotalLimit.Amount);
    }
}

public record LoginRequest(string Email, string Password);

public record RegisterRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string? Phone = null,
    bool IsDealer = false);

public record RefreshTokenRequest(string RefreshToken);

public record AuthResponse(
    string Token,
    string RefreshToken,
    DateTime ExpiresAt,
    UserDto User);

public record UserDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string[] Roles,
    bool IsDealer,
    decimal? CreditLimit);

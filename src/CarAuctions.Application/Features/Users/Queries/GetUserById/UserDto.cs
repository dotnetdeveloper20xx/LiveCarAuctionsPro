namespace CarAuctions.Application.Features.Users.Queries.GetUserById;

public record UserDto
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string? PhoneNumber { get; init; }
    public string Status { get; init; } = string.Empty;
    public string[] Roles { get; init; } = Array.Empty<string>();
    public bool IsDealer { get; init; }
    public string? DealerLicenseNumber { get; init; }
    public string? DealerCompanyName { get; init; }
    public decimal? CreditLimit { get; init; }
    public string? CreditLimitCurrency { get; init; }
    public DateTime CreatedAt { get; init; }
}

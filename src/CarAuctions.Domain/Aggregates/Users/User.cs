using CarAuctions.Domain.Aggregates.Users.Events;
using CarAuctions.Domain.Common;
using ErrorOr;

namespace CarAuctions.Domain.Aggregates.Users;

/// <summary>
/// User aggregate root.
/// </summary>
public sealed class User : AggregateRoot<UserId>
{
    public string Email { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string? Phone { get; private set; }
    public UserRole Roles { get; private set; }
    public UserStatus Status { get; private set; }
    public Address? Address { get; private set; }
    public CreditLimit? CreditLimit { get; private set; }
    public bool IsDealer { get; private set; }
    public string? DealerLicenseNumber { get; private set; }
    public string? CompanyName { get; private set; }
    public bool IsKycVerified { get; private set; }
    public DateTime? KycVerifiedAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public DateTime? LastLoginAt { get; private set; }

    public string FullName => $"{FirstName} {LastName}";

    private User(
        UserId id,
        string email,
        string firstName,
        string lastName,
        string? phone,
        UserRole roles) : base(id)
    {
        Email = email;
        FirstName = firstName;
        LastName = lastName;
        Phone = phone;
        Roles = roles;
        Status = UserStatus.Pending;
        IsDealer = roles.HasFlag(UserRole.Dealer);
        CreatedAt = DateTime.UtcNow;
    }

    private User() : base()
    {
        Email = string.Empty;
        FirstName = string.Empty;
        LastName = string.Empty;
    }

    public static ErrorOr<User> Create(
        string email,
        string firstName,
        string lastName,
        UserRole roles = UserRole.Buyer,
        string? phone = null)
    {
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            return Error.Validation("User.InvalidEmail", "Valid email is required.");

        if (string.IsNullOrWhiteSpace(firstName))
            return Error.Validation("User.FirstNameRequired", "First name is required.");

        if (string.IsNullOrWhiteSpace(lastName))
            return Error.Validation("User.LastNameRequired", "Last name is required.");

        var user = new User(
            UserId.CreateUnique(),
            email.Trim().ToLowerInvariant(),
            firstName.Trim(),
            lastName.Trim(),
            phone?.Trim(),
            roles);

        user.RaiseDomainEvent(new UserRegisteredEvent(user.Id, user.Email, user.Roles));

        return user;
    }

    public ErrorOr<Success> Activate()
    {
        if (Status != UserStatus.Pending)
            return Error.Conflict("User.NotPending", "User must be in pending status to activate.");

        Status = UserStatus.Active;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new UserActivatedEvent(Id));

        return Result.Success;
    }

    public ErrorOr<Success> Suspend(string reason)
    {
        if (Status != UserStatus.Active)
            return Error.Conflict("User.NotActive", "User must be active to suspend.");

        Status = UserStatus.Suspended;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new UserSuspendedEvent(Id, reason));

        return Result.Success;
    }

    public void SetCreditLimit(Money amount)
    {
        CreditLimit = CreditLimit.Create(amount);
        UpdatedAt = DateTime.UtcNow;
    }

    public ErrorOr<Success> ReserveCredit(Money amount)
    {
        if (CreditLimit is null)
            return Error.Conflict("User.NoCreditLimit", "User has no credit limit set.");

        var result = CreditLimit.Reserve(amount);
        if (result.IsError)
            return result.Errors;

        CreditLimit = result.Value;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success;
    }

    public void ReleaseCredit(Money amount)
    {
        if (CreditLimit is not null)
        {
            CreditLimit = CreditLimit.Release(amount);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void SetAddress(Address address)
    {
        Address = address;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddRole(UserRole role)
    {
        Roles |= role;
        IsDealer = Roles.HasFlag(UserRole.Dealer);
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveRole(UserRole role)
    {
        Roles &= ~role;
        IsDealer = Roles.HasFlag(UserRole.Dealer);
        UpdatedAt = DateTime.UtcNow;
    }

    public bool HasRole(UserRole role) => Roles.HasFlag(role);

    public bool CanBid() => Status == UserStatus.Active && HasRole(UserRole.Buyer);

    public bool CanSell() => Status == UserStatus.Active && (HasRole(UserRole.Seller) || HasRole(UserRole.Dealer));

    public ErrorOr<Success> VerifyKyc()
    {
        if (IsKycVerified)
            return Error.Conflict("User.AlreadyVerified", "User is already KYC verified.");

        IsKycVerified = true;
        KycVerifiedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new UserKycVerifiedEvent(Id));

        return Result.Success;
    }

    public void SetDealerInfo(string dealerLicenseNumber, string companyName)
    {
        DealerLicenseNumber = dealerLicenseNumber;
        CompanyName = companyName;
        IsDealer = true;
        AddRole(UserRole.Dealer);
        UpdatedAt = DateTime.UtcNow;
    }

    public void RecordLogin()
    {
        LastLoginAt = DateTime.UtcNow;
    }
}

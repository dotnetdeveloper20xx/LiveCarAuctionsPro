using CarAuctions.Domain.Common;

namespace CarAuctions.Domain.Aggregates.Users.Events;

public sealed class UserRegisteredEvent : DomainEventBase
{
    public UserId UserId { get; }
    public string Email { get; }
    public UserRole Roles { get; }

    public UserRegisteredEvent(UserId userId, string email, UserRole roles)
    {
        UserId = userId;
        Email = email;
        Roles = roles;
    }
}

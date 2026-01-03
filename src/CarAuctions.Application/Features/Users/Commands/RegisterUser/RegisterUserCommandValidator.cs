using FluentValidation;

namespace CarAuctions.Application.Features.Users.Commands.RegisterUser;

public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(100).WithMessage("First name must not exceed 100 characters");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(100).WithMessage("Last name must not exceed 100 characters");

        RuleFor(x => x.Roles)
            .NotEmpty().WithMessage("At least one role is required")
            .Must(BeValidRoles).WithMessage("One or more roles are invalid");

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\+?[\d\s-]{10,}$")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber))
            .WithMessage("Invalid phone number format");
    }

    private static bool BeValidRoles(string[] roles)
    {
        var validRoles = new[] { "Buyer", "Seller", "Dealer", "Admin", "Inspector" };
        return roles.All(r => validRoles.Contains(r, StringComparer.OrdinalIgnoreCase));
    }
}

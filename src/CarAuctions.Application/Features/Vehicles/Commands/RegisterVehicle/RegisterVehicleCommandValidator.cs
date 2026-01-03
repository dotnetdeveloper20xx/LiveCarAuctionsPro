using FluentValidation;

namespace CarAuctions.Application.Features.Vehicles.Commands.RegisterVehicle;

public class RegisterVehicleCommandValidator : AbstractValidator<RegisterVehicleCommand>
{
    public RegisterVehicleCommandValidator()
    {
        RuleFor(x => x.VIN)
            .NotEmpty().WithMessage("VIN is required")
            .Length(17).WithMessage("VIN must be exactly 17 characters");

        RuleFor(x => x.Make)
            .NotEmpty().WithMessage("Make is required")
            .MaximumLength(100).WithMessage("Make must not exceed 100 characters");

        RuleFor(x => x.Model)
            .NotEmpty().WithMessage("Model is required")
            .MaximumLength(100).WithMessage("Model must not exceed 100 characters");

        RuleFor(x => x.Year)
            .InclusiveBetween(1900, DateTime.UtcNow.Year + 1)
            .WithMessage("Year must be valid");

        RuleFor(x => x.Mileage)
            .GreaterThanOrEqualTo(0).WithMessage("Mileage cannot be negative");

        RuleFor(x => x.OwnerId)
            .NotEmpty().WithMessage("Owner is required");

        RuleFor(x => x.TitleStatus)
            .NotEmpty().WithMessage("Title status is required")
            .Must(BeValidTitleStatus).WithMessage("Invalid title status");
    }

    private static bool BeValidTitleStatus(string status)
    {
        return status.ToLower() switch
        {
            "clean" or "salvage" or "rebuilt" or "lemon" or "flood" => true,
            _ => false
        };
    }
}

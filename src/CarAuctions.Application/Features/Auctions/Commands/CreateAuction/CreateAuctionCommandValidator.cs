using FluentValidation;

namespace CarAuctions.Application.Features.Auctions.Commands.CreateAuction;

public class CreateAuctionCommandValidator : AbstractValidator<CreateAuctionCommand>
{
    public CreateAuctionCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters");

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Auction type is required")
            .Must(BeValidAuctionType).WithMessage("Invalid auction type");

        RuleFor(x => x.VehicleId)
            .NotEmpty().WithMessage("Vehicle is required");

        RuleFor(x => x.SellerId)
            .NotEmpty().WithMessage("Seller is required");

        RuleFor(x => x.StartingPrice)
            .GreaterThan(0).WithMessage("Starting price must be greater than 0");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required")
            .Length(3).WithMessage("Currency must be a 3-letter code");

        RuleFor(x => x.StartTime)
            .GreaterThan(DateTime.UtcNow.AddMinutes(-5))
            .WithMessage("Start time must be in the future");

        RuleFor(x => x.EndTime)
            .GreaterThan(x => x.StartTime)
            .WithMessage("End time must be after start time");

        RuleFor(x => x.ReservePrice)
            .GreaterThan(x => x.StartingPrice)
            .When(x => x.ReservePrice.HasValue)
            .WithMessage("Reserve price must be greater than starting price");

        RuleFor(x => x.BuyNowPrice)
            .GreaterThan(x => x.ReservePrice ?? x.StartingPrice)
            .When(x => x.BuyNowPrice.HasValue)
            .WithMessage("Buy now price must be greater than reserve or starting price");
    }

    private static bool BeValidAuctionType(string type)
    {
        return type.ToLower() switch
        {
            "timed" or "live" or "buynow" => true,
            _ => false
        };
    }
}

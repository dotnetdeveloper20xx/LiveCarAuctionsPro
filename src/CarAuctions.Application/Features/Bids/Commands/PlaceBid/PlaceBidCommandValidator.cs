using FluentValidation;

namespace CarAuctions.Application.Features.Bids.Commands.PlaceBid;

public class PlaceBidCommandValidator : AbstractValidator<PlaceBidCommand>
{
    public PlaceBidCommandValidator()
    {
        RuleFor(x => x.AuctionId)
            .NotEmpty().WithMessage("Auction is required");

        RuleFor(x => x.BidderId)
            .NotEmpty().WithMessage("Bidder is required");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Bid amount must be greater than 0");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required")
            .Length(3).WithMessage("Currency must be a 3-letter code");

        RuleFor(x => x.MaxProxyAmount)
            .GreaterThan(x => x.Amount)
            .When(x => x.IsProxy && x.MaxProxyAmount.HasValue)
            .WithMessage("Max proxy amount must be greater than initial bid");
    }
}

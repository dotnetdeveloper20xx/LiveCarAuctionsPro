using CarAuctions.Application.Common.Interfaces;
using CarAuctions.Domain.Aggregates.Auctions;
using CarAuctions.Domain.Aggregates.Payments;
using CarAuctions.Domain.Aggregates.Users;
using CarAuctions.Domain.Common;
using ErrorOr;
using MediatR;

namespace CarAuctions.Application.Features.Payments.Commands.InitiatePayment;

public class InitiatePaymentCommandHandler : IRequestHandler<InitiatePaymentCommand, ErrorOr<Guid>>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IAuctionRepository _auctionRepository;
    private readonly IUserRepository _userRepository;
    private readonly IPaymentGateway _paymentGateway;
    private readonly IDateTime _dateTime;

    public InitiatePaymentCommandHandler(
        IPaymentRepository paymentRepository,
        IAuctionRepository auctionRepository,
        IUserRepository userRepository,
        IPaymentGateway paymentGateway,
        IDateTime dateTime)
    {
        _paymentRepository = paymentRepository;
        _auctionRepository = auctionRepository;
        _userRepository = userRepository;
        _paymentGateway = paymentGateway;
        _dateTime = dateTime;
    }

    public async Task<ErrorOr<Guid>> Handle(
        InitiatePaymentCommand request,
        CancellationToken cancellationToken)
    {
        var auctionId = new AuctionId(request.AuctionId);
        var auction = await _auctionRepository.GetByIdAsync(auctionId, cancellationToken);

        if (auction is null)
        {
            return Error.NotFound("Auction.NotFound", "Auction not found");
        }

        if (auction.Status != AuctionStatus.Completed)
        {
            return Error.Validation("Auction.NotCompleted", "Payment can only be initiated for completed auctions");
        }

        var userId = new UserId(request.WinningBidderId);
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (user is null)
        {
            return Error.NotFound("User.NotFound", "User not found");
        }

        // Check for existing payment
        var existingPayment = await _paymentRepository.GetByAuctionIdAsync(auctionId, cancellationToken);
        if (existingPayment is not null && existingPayment.Status != PaymentStatus.Failed)
        {
            return Error.Conflict("Payment.Exists", "Payment already exists for this auction");
        }

        var amount = Money.Create(request.Amount, request.Currency);

        // Calculate fees
        var buyerFee = Fee.Calculate(amount, FeeType.BuyerPremium, 0.10m); // 10% buyer premium
        var totalAmount = Money.Create(amount.Amount + buyerFee.Amount.Amount, request.Currency);

        var paymentResult = Payment.Create(
            auctionId,
            userId,
            amount,
            buyerFee,
            _dateTime.UtcNow);

        if (paymentResult.IsError)
        {
            return paymentResult.Errors;
        }

        var payment = paymentResult.Value;

        // Initiate payment with gateway
        var gatewayResult = await _paymentGateway.InitiatePaymentAsync(
            payment.Id.Value,
            user.Email,
            totalAmount.Amount,
            totalAmount.Currency,
            $"Payment for Auction {auction.Title}",
            cancellationToken);

        if (gatewayResult.IsError)
        {
            payment.MarkAsFailed(gatewayResult.FirstError.Description);
        }
        else
        {
            payment.SetExternalReference(gatewayResult.Value);
        }

        await _paymentRepository.AddAsync(payment, cancellationToken);

        return payment.Id.Value;
    }
}

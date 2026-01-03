using CarAuctions.Application.Common.Interfaces;
using CarAuctions.Domain.Aggregates.Payments;
using ErrorOr;
using MediatR;

namespace CarAuctions.Application.Features.Payments.Commands.ProcessPayment;

public class ProcessPaymentCommandHandler : IRequestHandler<ProcessPaymentCommand, ErrorOr<Success>>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IInvoiceService _invoiceService;
    private readonly IDateTime _dateTime;

    public ProcessPaymentCommandHandler(
        IPaymentRepository paymentRepository,
        IInvoiceService invoiceService,
        IDateTime dateTime)
    {
        _paymentRepository = paymentRepository;
        _invoiceService = invoiceService;
        _dateTime = dateTime;
    }

    public async Task<ErrorOr<Success>> Handle(
        ProcessPaymentCommand request,
        CancellationToken cancellationToken)
    {
        var paymentId = new PaymentId(request.PaymentId);
        var payment = await _paymentRepository.GetByIdAsync(paymentId, cancellationToken);

        if (payment is null)
        {
            return Error.NotFound("Payment.NotFound", "Payment not found");
        }

        if (payment.Status == PaymentStatus.Completed)
        {
            return Error.Conflict("Payment.AlreadyCompleted", "Payment has already been completed");
        }

        if (request.IsSuccessful)
        {
            var completeResult = payment.Complete(request.ExternalTransactionId, _dateTime.UtcNow);
            if (completeResult.IsError)
            {
                return completeResult.Errors;
            }

            // Generate invoice
            await _invoiceService.GenerateInvoiceAsync(payment, cancellationToken);
        }
        else
        {
            payment.MarkAsFailed(request.FailureReason ?? "Payment failed");
        }

        await _paymentRepository.UpdateAsync(payment, cancellationToken);

        return Result.Success;
    }
}

using CarAuctions.Api.Authorization;
using CarAuctions.Application.Features.Payments.Commands.InitiatePayment;
using CarAuctions.Application.Features.Payments.Commands.ProcessPayment;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarAuctions.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly ISender _mediator;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(ISender mediator, ILogger<PaymentsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Initiate payment for a won auction
    /// </summary>
    [HttpPost("initiate")]
    [Authorize(Policy = AuthorizationPolicies.RequireBuyer)]
    [ProducesResponseType(typeof(PaymentInitiatedResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> InitiatePayment(
        [FromBody] InitiatePaymentRequest request,
        CancellationToken cancellationToken)
    {
        var command = new InitiatePaymentCommand(
            request.AuctionId,
            request.WinningBidderId,
            request.Amount,
            request.Currency);

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsError)
        {
            return result.FirstError.Type switch
            {
                ErrorOr.ErrorType.NotFound => NotFound(new { error = result.FirstError.Description }),
                ErrorOr.ErrorType.Conflict => Conflict(new { error = result.FirstError.Description }),
                _ => BadRequest(new { error = result.FirstError.Description })
            };
        }

        return Ok(new PaymentInitiatedResponse(result.Value));
    }

    /// <summary>
    /// Process payment webhook (called by payment gateway)
    /// </summary>
    [HttpPost("webhook")]
    [AllowAnonymous] // Webhooks are authenticated via signature
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ProcessPaymentWebhook(
        [FromBody] PaymentWebhookRequest request,
        CancellationToken cancellationToken)
    {
        // In production, verify webhook signature here
        _logger.LogInformation(
            "Payment webhook received for {PaymentId}: {Status}",
            request.PaymentId,
            request.IsSuccessful ? "Success" : "Failed");

        var command = new ProcessPaymentCommand(
            request.PaymentId,
            request.ExternalTransactionId,
            request.IsSuccessful,
            request.FailureReason);

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsError)
        {
            _logger.LogWarning(
                "Payment webhook processing failed for {PaymentId}: {Error}",
                request.PaymentId,
                result.FirstError.Description);

            return BadRequest(new { error = result.FirstError.Description });
        }

        return Ok();
    }

    /// <summary>
    /// Get payment status
    /// </summary>
    [HttpGet("{paymentId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.RequireBuyer)]
    [ProducesResponseType(typeof(PaymentStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPaymentStatus(
        Guid paymentId,
        CancellationToken cancellationToken)
    {
        // In production, implement GetPaymentQuery
        return Ok(new PaymentStatusResponse(paymentId, "Pending", null));
    }
}

public record InitiatePaymentRequest(
    Guid AuctionId,
    Guid WinningBidderId,
    decimal Amount,
    string Currency = "USD");

public record PaymentInitiatedResponse(Guid PaymentId);

public record PaymentWebhookRequest(
    Guid PaymentId,
    string ExternalTransactionId,
    bool IsSuccessful,
    string? FailureReason);

public record PaymentStatusResponse(
    Guid PaymentId,
    string Status,
    DateTime? CompletedAt);

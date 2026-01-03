using CarAuctions.Application.Features.Bids.Commands.PlaceBid;
using CarAuctions.Application.Features.Bids.Queries.GetBidHistory;
using CarAuctions.Api.Hubs;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace CarAuctions.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BidsController : ControllerBase
{
    private readonly ISender _mediator;
    private readonly IHubContext<AuctionHub> _hubContext;

    public BidsController(ISender mediator, IHubContext<AuctionHub> hubContext)
    {
        _mediator = mediator;
        _hubContext = hubContext;
    }

    [HttpGet("auction/{auctionId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<BidDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBidHistory(
        Guid auctionId,
        [FromQuery] int limit = 50,
        CancellationToken cancellationToken = default)
    {
        var query = new GetBidHistoryQuery(auctionId, limit);
        var result = await _mediator.Send(query, cancellationToken);

        return result.Match(
            success => Ok(success),
            errors => Problem(errors));
    }

    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PlaceBid(
        [FromBody] PlaceBidCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsError)
        {
            return Problem(result.Errors);
        }

        var bidId = result.Value;

        // Notify connected clients about the new bid
        await _hubContext.Clients.Group($"auction-{command.AuctionId}")
            .SendAsync("BidPlaced", new
            {
                BidId = bidId,
                AuctionId = command.AuctionId,
                Amount = command.Amount,
                Currency = command.Currency,
                Timestamp = DateTime.UtcNow
            }, cancellationToken);

        return Created($"/api/bids/{bidId}", bidId);
    }

    private IActionResult Problem(List<ErrorOr.Error> errors)
    {
        var firstError = errors.First();

        var statusCode = firstError.Type switch
        {
            ErrorOr.ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorOr.ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorOr.ErrorType.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };

        return Problem(
            statusCode: statusCode,
            title: firstError.Code,
            detail: firstError.Description);
    }
}

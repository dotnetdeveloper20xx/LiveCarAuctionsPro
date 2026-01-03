using CarAuctions.Application.Common.Models;
using CarAuctions.Application.Features.Auctions.Commands.CloseAuction;
using CarAuctions.Application.Features.Auctions.Commands.CreateAuction;
using CarAuctions.Application.Features.Auctions.Commands.StartAuction;
using CarAuctions.Application.Features.Auctions.Queries.GetAuctionById;
using CarAuctions.Application.Features.Auctions.Queries.GetAuctions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CarAuctions.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuctionsController : ControllerBase
{
    private readonly ISender _mediator;

    public AuctionsController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PaginatedList<AuctionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAuctions(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? status = null,
        [FromQuery] string? type = null,
        [FromQuery] bool? dealerOnly = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAuctionsQuery(pageNumber, pageSize, status, type, dealerOnly);
        var result = await _mediator.Send(query, cancellationToken);

        return result.Match(
            success => Ok(success),
            errors => Problem(errors));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AuctionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAuction(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetAuctionByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);

        return result.Match(
            success => Ok(success),
            errors => Problem(errors));
    }

    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateAuction(
        [FromBody] CreateAuctionCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);

        return result.Match(
            id => CreatedAtAction(nameof(GetAuction), new { id }, id),
            errors => Problem(errors));
    }

    [HttpPost("{id:guid}/start")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> StartAuction(Guid id, CancellationToken cancellationToken)
    {
        var command = new StartAuctionCommand(id);
        var result = await _mediator.Send(command, cancellationToken);

        return result.Match(
            _ => Ok(),
            errors => Problem(errors));
    }

    [HttpPost("{id:guid}/close")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CloseAuction(Guid id, CancellationToken cancellationToken)
    {
        var command = new CloseAuctionCommand(id);
        var result = await _mediator.Send(command, cancellationToken);

        return result.Match(
            _ => Ok(),
            errors => Problem(errors));
    }

    private IActionResult Problem(List<ErrorOr.Error> errors)
    {
        var firstError = errors.First();

        var statusCode = firstError.Type switch
        {
            ErrorOr.ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorOr.ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorOr.ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorOr.ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorOr.ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status500InternalServerError
        };

        return Problem(
            statusCode: statusCode,
            title: firstError.Code,
            detail: firstError.Description);
    }
}

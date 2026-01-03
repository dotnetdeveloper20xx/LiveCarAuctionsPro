using Microsoft.AspNetCore.Mvc;

namespace CarAuctions.Api.Controllers;

/// <summary>
/// Controller for auction operations.
/// </summary>
public class AuctionsController : ApiControllerBase
{
    /// <summary>
    /// Gets all auctions.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        // Placeholder - will be implemented in Phase 4 with CQRS
        return Ok(new { Message = "Auctions endpoint ready", Timestamp = DateTime.UtcNow });
    }

    /// <summary>
    /// Gets an auction by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        // Placeholder - will be implemented in Phase 4
        return Ok(new { Id = id, Message = "Auction detail endpoint ready" });
    }
}

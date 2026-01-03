using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CarAuctions.Api.Controllers;

/// <summary>
/// Base controller providing common functionality for all API controllers.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
    private ISender? _mediator;

    /// <summary>
    /// Gets the MediatR sender instance.
    /// </summary>
    protected ISender Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();
}

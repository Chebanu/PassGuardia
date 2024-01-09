using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using PassGuardia.Api.Constants;
using PassGuardia.Domain.Queries;

namespace PassGuardia.Api.Controllers;

[Route("[controller]")]
public class AuditController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuditController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [Route("")]
    [Authorize(Policy = AuthorizePolicies.Admin)]
    public async Task<IActionResult> GetAudits(CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetAuditLogQuery(), cancellationToken);
        return Ok(result);
    }
}
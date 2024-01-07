using System.Runtime.CompilerServices;

using MediatR;

using Microsoft.AspNetCore.Mvc;

using PassGuardia.Contracts.Http;
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
    public async Task<IActionResult> GetAudits(CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetAuditLogQuery(), cancellationToken);

        if(result == null)
        {
            return BadRequest(new ErrorResponse
            {
                Errors = ["Nothing has found"]
            });
        }

        return Ok(result);
    }
}
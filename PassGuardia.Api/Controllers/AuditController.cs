using FluentValidation;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using PassGuardia.Api.Constants;
using PassGuardia.Contracts.Http;
using PassGuardia.Domain.Queries;

namespace PassGuardia.Api.Controllers;

[Route("audit")]
public class AuditController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IValidator<AuditRequest> _auditValidator;

    public AuditController(IMediator mediator, IValidator<AuditRequest> auditValidator)
    {
        _mediator = mediator;
        _auditValidator = auditValidator;
    }

    [HttpGet]
    [Route("")]
    [ProducesResponseType(typeof(ErrorResponse), 400)]
    [ProducesResponseType(typeof(ErrorResponse), 500)]
    public async Task<IActionResult> GetAudit(AuditRequest auditRequest, CancellationToken cancellationToken = default)
    {
        var validationResult = await _auditValidator.ValidateAsync(auditRequest, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(new ErrorResponse
            {
                Errors = validationResult.Errors.Select(x => x.ErrorMessage)
            });
        }

        var propAudit = new GetAuditLogQuery
        {
            PageNumber = auditRequest.PageNumber,
            PageSize = auditRequest.PageSize
        };

        var result = await _mediator.Send(propAudit, cancellationToken);
        return Ok(result);
    }
}
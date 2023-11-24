using MediatR;
using Microsoft.AspNetCore.Mvc;
using PassGuardia.Contracts.DTO;
using PassGuardia.Domain.Commands;
using PassGuardia.Domain.Queries;
using PassGuardia.DTO;

namespace PassGuardia.Api.Controllers;

[Route("[controller]")]
public class PasswordController : ControllerBase
{
    private readonly IMediator _mediator;

    public PasswordController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetPassword(Guid id, CancellationToken cancellationToken = default)
    {
        GetPasswordByIdQuery query = new GetPasswordByIdQuery
        {
            Id = id
        };

        GetPasswordByIdResult result = await _mediator.Send(query, cancellationToken);

        return Ok(new ResponsePassword
        {
            Password = result.Password
        });
    }

    [HttpPost]
    public async Task<IActionResult> CreatePassword(RequestPassword requestPassword, CancellationToken cancellationToken = default)
    {
        CreatePasswordCommand command = new CreatePasswordCommand
        {
            Password = requestPassword.EncryptedPassword
        };

        CreatePasswordResult result = await _mediator.Send(command, cancellationToken);

        return Created($"tasks/{result.Id}", new CreatePasswordResult
        {
            Id = result.Id
        });
    }
}

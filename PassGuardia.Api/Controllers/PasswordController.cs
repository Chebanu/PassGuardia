using MediatR;

using Microsoft.AspNetCore.Mvc;

using PassGuardia.Contracts.DTO;
using PassGuardia.Contracts.Http;
using PassGuardia.Domain.Commands;
using PassGuardia.Domain.Queries;

namespace PassGuardia.Api.Controllers;

[Route("passwords")]
public class PasswordController : ControllerBase
{
    private readonly IMediator _mediator;

    public PasswordController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(GetPasswordByIdResult), 200)]
    [ProducesResponseType(typeof(ErrorModel), 400)]
    [ProducesResponseType(typeof(ErrorModel), 404)]
    public async Task<IActionResult> GetPassword(Guid id, CancellationToken cancellationToken = default)
    {
        GetPasswordByIdQuery query = new()
        {
            Id = id
        };

        var result = await _mediator.Send(query, cancellationToken);
        if (result?.Password == null)
        {
            return NotFound(new ErrorModel { Message = $"Password with id {id} not found" });
        }

        return Ok(new ResponsePassword { Password = result.Password });
    }

    [HttpPost]
    [ProducesResponseType(typeof(CreatePasswordResult), 201)]
    [ProducesResponseType(typeof(ErrorModel), 400)]
    public async Task<IActionResult> CreatePassword(RequestPassword requestPassword, CancellationToken cancellationToken = default)
    {
        CreatePasswordCommand command = new()
        {
            Password = requestPassword.Password
        };

        CreatePasswordResult result = await _mediator.Send(command, cancellationToken);

        return Created($"password/{result.Id}", new CreatePasswordResult
        {
            Id = result.Id
        });
    }
}
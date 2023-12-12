using MediatR;

using Microsoft.AspNetCore.Mvc;

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

    [HttpGet]
    [Route("{id}")]
    [ProducesResponseType(typeof(GetPasswordByIdResult), 200)]
    [ProducesResponseType(typeof(ErrorModel), 400)]
    [ProducesResponseType(typeof(ErrorModel), 404)]
    public async Task<IActionResult> GetPassword([FromRoute] Guid id, CancellationToken cancellationToken = default)
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
    [Route("")]
    [ProducesResponseType(typeof(CreatePasswordResult), 201)]
    [ProducesResponseType(typeof(ErrorModel), 400)]
    public async Task<IActionResult> CreatePassword([FromBody] RequestPassword requestPassword, CancellationToken cancellationToken = default)
    {
        try
        {
            CreatePasswordCommand command = new()
            {
                Password = requestPassword.Password
            };

            CreatePasswordResult result = await _mediator.Send(command, cancellationToken);

            return Created($"password/{result.PasswordId}", new CreatePasswordResult
            {
                PasswordId = result.PasswordId
            });
        }
        catch (ArgumentOutOfRangeException)
        {
            return BadRequest(new ErrorModel { Message = "You are out of range. Password must be in range between 1-100 characters" });
        }
        catch (ArgumentException)
        {
            return BadRequest(new ErrorModel { Message = "Input is wrong" });
        }
        catch (Exception)
        {
            return BadRequest(new ErrorModel { Message = "Something went wrong" });
        }
    }
}
using FluentValidation;

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
    private readonly IValidator<PasswordRequest> _passwordValidator;

    public PasswordController(IMediator mediator, IValidator<PasswordRequest> passwordValidator)
    {
        _mediator = mediator;
        _passwordValidator = passwordValidator;
    }

    [HttpGet]
    [Route("{id}")]
    [ProducesResponseType(typeof(GetPasswordByIdResult), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 400)]
    [ProducesResponseType(typeof(ErrorResponse), 404)]
    public async Task<IActionResult> GetPassword([FromRoute] Guid id, CancellationToken cancellationToken = default)
    {
        GetPasswordByIdQuery query = new()
        {
            Id = id
        };

        var result = await _mediator.Send(query, cancellationToken);

        if (result?.Password == null)
        {
            return NotFound(new ErrorResponse { Errors = new[] { $"Password with id \"{id}\" not found" } });
        }

        return Ok(new PasswordResponse { Password = result.Password });
    }

    [HttpPost]
    [Route("")]
    [ProducesResponseType(typeof(CreatePasswordResult), 201)]
    [ProducesResponseType(typeof(ErrorResponse), 400)]
    public async Task<IActionResult> CreatePassword([FromBody] PasswordRequest passwordRequest, CancellationToken cancellationToken = default)
    {
        var validationResult = await _passwordValidator.ValidateAsync(passwordRequest, cancellationToken);

        if (!validationResult.IsValid)
        {
            return BadRequest(new ErrorResponse
            {
                Errors = validationResult.Errors.Select(e => e.ErrorMessage).ToArray()
            });
        }

        CreatePasswordCommand command = new()
        {
            Password = passwordRequest.Password
        };

        CreatePasswordResult passwordResult = await _mediator.Send(command, cancellationToken);

        return Created($"passwords/{passwordResult.PasswordId}", new CreatePasswordResult
        {
            PasswordId = passwordResult.PasswordId
        });
    }
}
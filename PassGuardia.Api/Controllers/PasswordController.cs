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

        return Ok(new PasswordResponse { Password = result.Password });
    }

    [HttpPost]
    [Route("")]
    [ProducesResponseType(typeof(CreatePasswordResult), 201)]
    [ProducesResponseType(typeof(ErrorModel), 400)]
    [ProducesResponseType(typeof(ErrorModel), 500)]
    public async Task<IActionResult> CreatePassword([FromBody] PasswordRequest requestPassword, CancellationToken cancellationToken = default)
    {
        var validationResult = await _passwordValidator.ValidateAsync(requestPassword, cancellationToken);

        if (!validationResult.IsValid)
        {
            return BadRequest(new ErrorModel { Message = "Input is wrong" });
        }

        try
        {
            CreatePasswordCommand command = new()
            {
                Password = requestPassword.Password
            };

            CreatePasswordResult passwordResult = await _mediator.Send(command, cancellationToken);

            return Created($"password/{passwordResult.PasswordId}", new CreatePasswordResult
            {
                PasswordId = passwordResult.PasswordId
            });
        }
        catch (Exception)
        {
            return StatusCode(500, new ErrorModel { Message = "Something went wrong" });
        }
    }
}
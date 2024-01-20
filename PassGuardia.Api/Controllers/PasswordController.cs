using FluentValidation;

using MediatR;

using Microsoft.AspNetCore.Authorization;
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
    private readonly IValidator<UpdatePasswordVisibilityRequest> _updPasswordValidator;

    public PasswordController(IMediator mediator, IValidator<PasswordRequest> passwordValidator,
                                IValidator<UpdatePasswordVisibilityRequest> updPasswordValidator)
    {
        _mediator = mediator;
        _passwordValidator = passwordValidator;
        _updPasswordValidator = updPasswordValidator;
    }

    [HttpGet]
    [Route("{id}")]
    [ProducesResponseType(typeof(GetPasswordByIdResult), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 400)]
    [ProducesResponseType(typeof(ErrorResponse), 404)]
    public async Task<IActionResult> GetPassword([FromRoute] Guid id, CancellationToken cancellationToken = default)
    {
        var username = HttpContext.User.Identity?.Name;

        GetPasswordByIdQuery query = new()
        {
            Id = id,
            User = username
        };

        var result = await _mediator.Send(query, cancellationToken);

        if (result?.Password == null)
        {
            return BadRequest(new ErrorResponse { Errors = new[] { $"Password Not Found Or Forbidden To Access" } });
        }

        return Ok(new PasswordResponse { Password = result.Password, GetVisibility = result.GetVisibility });
    }

    [HttpPost]
    [Route("")]
    [Authorize]
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
            User = User.Identity.Name,
            Password = passwordRequest.Password,
            GetVisibility = passwordRequest.GetVisibility
        };

        CreatePasswordResult passwordResult = await _mediator.Send(command, cancellationToken);

        return Created($"passwords/{passwordResult.PasswordId}", new CreatePasswordResult
        {
            PasswordId = passwordResult.PasswordId
        });
    }

    [HttpPut]
    [Route("")]
    [Authorize]
    [ProducesResponseType(typeof(CreatePasswordResult), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 400)]
    public async Task<IActionResult> UpdatePasswordVisibility([FromBody] UpdatePasswordVisibilityRequest updatePassword,
                                                                        CancellationToken cancellationToken = default)
    {
        var validationResult = await _updPasswordValidator.ValidateAsync(updatePassword, cancellationToken);

        if (!validationResult.IsValid)
        {
            return BadRequest(new ErrorResponse
            {
                Errors = validationResult.Errors.Select(e => e.ErrorMessage).ToArray()
            });
        }
        UpdatePasswordVisibilityCommand command = new()
        {
            User = User.Identity.Name,
            PasswordVisibility = updatePassword
        };

        UpdatePasswordVisibilityResult passwordResult = await _mediator.Send(command, cancellationToken);

        return !passwordResult.Success ?
            BadRequest(new ErrorResponse
            {
                Errors = passwordResult.Errors
            })
            : NoContent();
    }
}
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

    /// <summary>
    /// Return decrypted password 
    /// </summary>
    /// <param name="id">Password ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Decrypted password</returns>
    /// <remarks>
    /// GET /passwords/12345678-1234-1234-1234-123456789012
    /// </remarks>
    /// <response code="200">Password found</response>
    /// <response code="400">Password Not Found Or Forbidden To Access</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="500">Internal server error</response>
    [HttpGet]
    [Route("{id}")]
    [ProducesResponseType(typeof(PasswordResponse), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 400)]
    [ProducesResponseType(typeof(ErrorResponse), 401)]
    [ProducesResponseType(typeof(ErrorResponse), 404)]
    [ProducesResponseType(typeof(ErrorResponse), 500)]
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

    /// <summary>
    /// Create new password
    /// </summary>
    /// <param name="passwordRequest">Create password</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created password $password/12345678-1234-1234-1234-123456789012</returns>
    /// <remarks>
    /// Sample request
    /// 
    /// POST /passwords
    /// {
    ///     details: "Test Password Details"
    /// }
    /// 
    /// </remarks>
    /// <response code="201">Password created</response>
    /// <response code="400">Invalid request</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden</response>
    /// <response code="500">Internal server error</response>
    [HttpPost]
    [Route("")]
    [Authorize]
    [ProducesResponseType(typeof(CreatePasswordResponse), 201)]
    [ProducesResponseType(typeof(ErrorResponse), 400)]
    [ProducesResponseType(typeof(ErrorResponse), 401)]
    [ProducesResponseType(typeof(ErrorResponse), 403)]
    [ProducesResponseType(typeof(ErrorResponse), 500)]
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

    /// <summary>
    /// Update password
    /// </summary>
    /// <param name="id">Password Id</param>
    /// <param name="updatePassword">Update password request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No Content</returns>
    /// <response code="204">Password updated</response>
    /// <response code="400">Invalid request</response>
    /// <response code="404">Password not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPut]
    [Route("{id}")]
    [Authorize]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ErrorResponse), 400)]
    [ProducesResponseType(typeof(ErrorResponse), 404)]
    [ProducesResponseType(typeof(ErrorResponse), 500)]
    public async Task<IActionResult> UpdatePassword([FromRoute] Guid id,[FromBody] UpdatePasswordRequest updatePassword,
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
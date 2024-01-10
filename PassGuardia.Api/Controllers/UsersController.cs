using System.Net.Mime;

using FluentValidation;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using PassGuardia.Api.Constants;
using PassGuardia.Api.Extensions;
using PassGuardia.Contracts.Http;
using PassGuardia.Domain.Commands;

namespace PassGuardia.Api.Controllers;

[Route("users")]
[ApiController]
[Consumes(MediaTypeNames.Application.Json)]
[Produces(MediaTypeNames.Application.Json)]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IValidator<RegisterUserRequest> _registerUserValidator;
    private readonly IValidator<AuthenticateUserRequest> _authenticateUserValidator;
    private readonly IValidator<AdminUpdateUserRequest> _adminUpdateUserValidator;

    public UsersController(IMediator mediator,
                            IValidator<RegisterUserRequest> registerUserValidator,
                            IValidator<AuthenticateUserRequest> authenticateUserValidator,
                            IValidator<AdminUpdateUserRequest> adminUpdateUserValidator)
    {
        _mediator = mediator;
        _registerUserValidator = registerUserValidator;
        _authenticateUserValidator = authenticateUserValidator;
        _adminUpdateUserValidator = adminUpdateUserValidator;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ErrorResponse), 400)]
    [ProducesResponseType(typeof(ErrorResponse), 500)]
    public async Task<IActionResult> RegisterUser([FromBody] RegisterUserRequest request,
                                                    CancellationToken cancellationToken = default)
    {
        var validationResult = await _registerUserValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(new ErrorResponse
            {
                Errors = validationResult.Errors.Select(x => x.ErrorMessage).ToArray()
            });
        }

        var command = new RegisterUserCommand
        {
            Username = request.Username,
            Password = request.Password
        };

        var result = await _mediator.Send(command, cancellationToken);

        return !result.Success ? BadRequest(new ErrorResponse
        {
            Errors = result.Errors.Select(x => x.Description).ToArray()
        })
            : Ok(new RegisterUserResponse
            {
                UserId = result.UserId
            });
    }

    [HttpPost("authenticate")]
    [ProducesResponseType(typeof(AuthenticateUserResponse), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 400)]
    [ProducesResponseType(typeof(ErrorResponse), 500)]
    public async Task<IActionResult> AuthenticateUser([FromBody] AuthenticateUserRequest request, CancellationToken cancellationToken = default)
    {
        var validationResult = await _authenticateUserValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(new ErrorResponse
            {
                Errors = validationResult.Errors.Select(x => x.ErrorMessage)
            });
        }

        var command = new AuthenticateUserCommand
        {
            Username = request.Username,
            Password = request.Password
        };

        var result = await _mediator.Send(command, cancellationToken);
        return !result.Success ?
            BadRequest(new ErrorResponse
            {
                Errors = result.Errors
            })
            : Ok(new AuthenticateUserResponse
            {
                Token = result.Token,
            });
    }

    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(MeResponse), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 401)]
    [ProducesResponseType(typeof(ErrorResponse), 500)]
    public IActionResult GetCurrentUser()
    {
        return Ok(new MeResponse
        {
            Username = User.Identity.Name,
            Roles = User.Claims.Roles().ToArray()
        });
    }

    [HttpPut("admin")]
    [Authorize(Policy = AuthorizePolicies.Admin)]
    [ProducesResponseType(typeof(ErrorResponse), 401)]
    [ProducesResponseType(typeof(ErrorResponse), 403)]
    [ProducesResponseType(typeof(ErrorResponse), 500)]
    public async Task<IActionResult> AdminUpdateUser([FromBody] AdminUpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        var validationResult = await _adminUpdateUserValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(new ErrorResponse
            {
                Errors = validationResult.Errors.Select(x => x.ErrorMessage)
            });
        }

        var command = new AdminUpdateUserCommand
        {
            Username = request.Username,
            RoleToAdd = request.Roles.Where(x => x.Action is UpdateRoleAction.Add).Select(x => x.Role).ToArray(),
            RoleToRemove = request.Roles.Where(x => x.Action is UpdateRoleAction.Remove).Select(x => x.Role).ToArray()
        };

        var result = await _mediator.Send(command, cancellationToken);
        return !result.Success
            ? BadRequest(new ErrorResponse
            {
                Errors = result.Errors
            })
            : Ok();
    }
}
using MediatR;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

using PassGuardia.Domain.Constants;
using PassGuardia.Domain.Handlers;

namespace PassGuardia.Domain.Commands;

public class RegisterUserCommand : IRequest<RegisterUserCommandResult>
{
    public required string Username { get; init; }
    public required string Password { get; init; }
}

public class RegisterUserCommandResult
{
    public string UserId { get; init; }
    public bool Success { get; init; }
    public IEnumerable<IdentityError> Errors { get; init; }
}

internal class RegisterUserCommandHandler : BaseRequestHandler<RegisterUserCommand, RegisterUserCommandResult>
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public RegisterUserCommandHandler(UserManager<IdentityUser> userManager,
                                        RoleManager<IdentityRole> roleManager,
                                        ILogger<BaseRequestHandler<RegisterUserCommand, RegisterUserCommandResult>> logger) : base(logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    protected override async Task<RegisterUserCommandResult> HandleInternal(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var user = new IdentityUser
        {
            UserName = request.Username
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return new RegisterUserCommandResult
            {
                Success = false,
                Errors = result.Errors
            };
        }

        if (!await _roleManager.RoleExistsAsync(Roles.User))
        {
            await _roleManager.CreateAsync(new IdentityRole(Roles.User));
        }

        result = await _userManager.AddToRoleAsync(user, Roles.User);

        return new RegisterUserCommandResult
        {
            UserId = user.Id,
            Success = result.Succeeded,
            Errors = result.Errors
        };

    }
}
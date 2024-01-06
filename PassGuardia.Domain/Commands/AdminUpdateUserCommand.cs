using MediatR;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

using PassGuardia.Domain.Handlers;

namespace PassGuardia.Domain.Commands;

public class AdminUpdateUserCommand : IRequest<AdminUpdateUserCommandResult>
{
    public required string Username { get; init; }
    public string[] RoleToAdd { get; init; } = [];
    public string[] RoleToRemove { get; init; } = [];
}

public class AdminUpdateUserCommandResult
{
    public bool Success { get; init; }
    public IEnumerable<string> Errors { get; init; }
}

internal class AdminUpdateUserCommandHandler : BaseRequestHandler<AdminUpdateUserCommand, AdminUpdateUserCommandResult>
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public AdminUpdateUserCommandHandler(UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ILogger<AdminUpdateUserCommandHandler> logger) : base(logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    protected override async Task<AdminUpdateUserCommandResult> HandleInternal(AdminUpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByNameAsync(request.Username);
        if (user == null)
        {
            return new AdminUpdateUserCommandResult
            {
                Success = false,
                Errors = ["User not found."]
            };
        }

        var roles = await _userManager.GetRolesAsync(user);

        var rolesToAdd = request.RoleToAdd.Except(roles).ToArray();
        var rolesToRemove = request.RoleToRemove.Intersect(roles).ToArray();

        if (rolesToAdd.Length != 0)
        {
            foreach (var roleToAdd in rolesToAdd)
            {
                if (!await _roleManager.RoleExistsAsync(roleToAdd))
                {
                    await _roleManager.CreateAsync(new IdentityRole(roleToAdd));
                }
            }

            var result = await _userManager.AddToRolesAsync(user, request.RoleToAdd.Except(roles));
            if (!result.Succeeded)
            {
                return new AdminUpdateUserCommandResult
                {
                    Success = false,
                    Errors = result.Errors.Select(x => x.Description)
                };
            }
        }

        if (request.RoleToRemove.Length != 0)
        {
            var result = await _userManager.RemoveFromRolesAsync(user, request.RoleToRemove.Intersect(roles));
            if (!result.Succeeded)
            {
                return new AdminUpdateUserCommandResult
                {
                    Success = false,
                    Errors = result.Errors.Select(x => x.Description)
                };
            }
        }

        return new AdminUpdateUserCommandResult
        {
            Success = true
        };
    }
}
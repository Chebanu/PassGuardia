using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using MediatR;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

using PassGuardia.Domain.Configuration;
using PassGuardia.Domain.Constants;
using PassGuardia.Domain.Handlers;

namespace PassGuardia.Domain.Commands;

public class AuthenticateUserCommand : IRequest<AuthenticateUserCommandResult>
{
    public required string Username { get; init; }
    public required string Password { get; init; }
}

public class AuthenticateUserCommandResult
{
    public bool Success { get; init; }
    public string Token { get; init; }
    public IEnumerable<string> Errors { get; init; }
}

internal class AuthenticateUserCommandHandler : BaseRequestHandler<AuthenticateUserCommand, AuthenticateUserCommandResult>
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IOptionsMonitor<JwtSettings> _jwtOptions;

    public AuthenticateUserCommandHandler(UserManager<IdentityUser> userManager,
                                            IOptionsMonitor<JwtSettings> jwtOptions,
                                            ILogger<AuthenticateUserCommandHandler> logger) : base(logger)
    {
        _userManager = userManager;
        _jwtOptions = jwtOptions;
    }


    protected override async Task<AuthenticateUserCommandResult> HandleInternal(AuthenticateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByNameAsync(request.Username);
        if (user == null)
        {
            return new AuthenticateUserCommandResult
            {
                Success = false,
                Errors = ["Invalid username or password"]
            };
        }

        var result = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!result)
        {
            return new AuthenticateUserCommandResult
            {
                Success = false,
                Errors = ["Invalid username or password."]
            };
        }

        var roles = await _userManager.GetRolesAsync(user);

        var token = GenerateToken(user, roles);

        return new AuthenticateUserCommandResult
        {
            Success = true,
            Token = token
        };
    }

    private string GenerateToken(IdentityUser user, IEnumerable<string> roles)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Name, user.UserName)
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimNames.Role, role));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.CurrentValue.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expires = DateTimeOffset.UtcNow.Add(_jwtOptions.CurrentValue.ExpiresIn);

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.CurrentValue.Issuer,
            audience: _jwtOptions.CurrentValue.Audience,
            claims: claims,
            expires: expires.UtcDateTime,
            signingCredentials: creds
            );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
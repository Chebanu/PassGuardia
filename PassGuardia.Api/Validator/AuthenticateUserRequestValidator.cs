using FluentValidation;

using PassGuardia.Api.Extensions;
using PassGuardia.Contracts.Http;

internal class AuthenticateUserRequestValidator : AbstractValidator<AuthenticateUserRequest>
{
    public AuthenticateUserRequestValidator()
    {
        this.RuleForUsername(x => x.Username);
        this.RuleForPassword(x => x.Password);
    }
}
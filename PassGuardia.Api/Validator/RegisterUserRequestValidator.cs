using FluentValidation;

using PassGuardia.Api.Extensions;
using PassGuardia.Contracts.Http;

namespace PassGuardia.Api.Validator;

internal class RegisterUserRequestValidator : AbstractValidator<RegisterUserRequest>
{
    public RegisterUserRequestValidator()
    {
        this.RuleForUsername(x => x.Username);
        this.RuleForPassword(x => x.Password);
    }
}
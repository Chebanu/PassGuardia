using FluentValidation;

using PassGuardia.Contracts.Http;

namespace PassGuardia.Api.Validator;

public class PasswordRequestValidator : AbstractValidator<PasswordRequest>
{
    public PasswordRequestValidator()
    {
        RuleFor(x => x.Password)
            .Length(1, 100)
            .WithMessage("Your password must be in 1 - 100 range characters");
    }
}
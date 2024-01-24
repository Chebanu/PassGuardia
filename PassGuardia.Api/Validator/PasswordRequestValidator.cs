using FluentValidation;

using PassGuardia.Contracts.Http;

namespace PassGuardia.Api.Validator;

public class PasswordRequestValidator : AbstractValidator<PasswordRequest>
{
    public PasswordRequestValidator()
    {
        RuleFor(x => x.Password)
            .NotNull()
            .WithMessage("The field is null")
            .Length(1, 100)
            .WithMessage("Your password must be in the 1-100 character range");

        RuleFor(x => x.GetVisibility).NotNull();
    }
}
using FluentValidation;

using PassGuardia.Contracts.Http;

namespace PassGuardia.Api.Validator;

public class PasswordRequestValidator : AbstractValidator<PasswordRequest>
{
    public PasswordRequestValidator()
    {
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MaximumLength(100)
            .WithMessage("Your password contains more than 100 characters.");
    }
}
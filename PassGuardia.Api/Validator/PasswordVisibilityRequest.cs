using FluentValidation;

using PassGuardia.Contracts.Http;

namespace PassGuardia.Api.Validator;

public class PasswordVisibilityRequest : AbstractValidator<UpdatePasswordVisibilityRequest>
{
    public PasswordVisibilityRequest()
    {
        RuleFor(x => x.Id)
            .NotNull()
            .WithMessage("'Id' is empty");

        RuleFor(x => x.GetVisibility).NotNull();
    }
}
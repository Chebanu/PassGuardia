using FluentValidation;

using PassGuardia.Api.Extensions;
using PassGuardia.Contracts.Http;

namespace PassGuardia.Api.Validator;

public class PasswordUpdateRequest : AbstractValidator<UpdatePasswordRequest>
{
    public PasswordUpdateRequest()
    {
        RuleFor(x => x.Visibility).IsInEnum();

        this.RuleForUpdateShareableUserList(x => x.ShareableList);
    }
}
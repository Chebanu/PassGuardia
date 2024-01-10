using FluentValidation;

using PassGuardia.Api.Extensions;
using PassGuardia.Contracts.Http;


namespace PassGuardia.Api.Validator;

internal class AdminUpdateUserRequestValidator : AbstractValidator<AdminUpdateUserRequest>
{
    public const string AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    public static string[] AvailableRoles = ["user", "admin"];

    public AdminUpdateUserRequestValidator()
    {
        this.RuleForUsername(x => x.Username);

        RuleFor(x => x.Roles)
            .NotNull()
            .NotEmpty()
            .Must(x => x == null || x.All(c => AvailableRoles.Contains(c.Role)))
            .WithMessage($"Roles must only contain the following roles: {AvailableRoles}")
            .Must(x => x == null || x.GroupBy(c => c.Role).All(c => c.Count() == 1))
            .WithMessage($"Roles must not contain duplicate roles");
    }
}
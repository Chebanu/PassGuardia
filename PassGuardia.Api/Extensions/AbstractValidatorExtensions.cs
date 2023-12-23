using FluentValidation;
using System.Linq.Expressions;

namespace PassGuardia.Api.Extensions;

internal static class AbstractValidatorExtensions
{
    public const string AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";

    public static void RuleForUsername<T>(this AbstractValidator<T> validator, Expression<Func<T, string>> expression)
    {
        validator.RuleFor(expression)
            .NotEmpty()
            .MaximumLength(256)
            .Must(x => x == null || x.All(c => AllowedUserNameCharacters.Contains(c)))
            .WithMessage($"Username must only contain the following characters: {AllowedUserNameCharacters}");
    }

    public static void RuleForPassword<T>(this AbstractValidator<T> validator, Expression<Func<T, string>> expression)
    {
        validator.RuleFor(expression)
            .NotEmpty()
            .MinimumLength(8);
    }
}

using System.Linq.Expressions;

using FluentValidation;

namespace PassGuardia.Api.Extensions;

internal static class AbstractValidatorExtensions
{
    public const string AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_";

    public const string AllowedPasswordCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*()-_=+?.,";

    public static void RuleForUsername<T>(this AbstractValidator<T> validator, Expression<Func<T, string>> expression)
    {
        validator.RuleFor(expression)
            .NotEmpty()
            .Length(5, 20)
            .Must(x => x == null || x.All(c => AllowedUserNameCharacters.Contains(c)))
            .WithMessage($"Username must only contain the following characters: {AllowedUserNameCharacters}.");
    }

    public static void RuleForPassword<T>(this AbstractValidator<T> validator, Expression<Func<T, string>> expression)
    {
        validator.RuleFor(expression)
            .NotEmpty()
            .Length(8, 100)
            .Must(x => x == null || x.All(c => AllowedPasswordCharacters.Contains(c)))
            .WithMessage($"Password must only contain the following characters: {AllowedPasswordCharacters}.");
    }

    public static void RuleForShareableUserList<T>(this AbstractValidator<T> validator, Expression<Func<T, List<string>>> expression)
    {
        validator.RuleFor(expression)
            .Must(list => list == null || list.Any())
            .WithMessage("The list must not be empty.")
            .When(list => list != null)

            .ForEach(rule => rule
                .Length(5, 20)
                .Must(userName => userName.All(c => AllowedUserNameCharacters.Contains(c)))
                .WithMessage($"Each username must only contain the following characters: {AllowedUserNameCharacters}.")
            );
    }

    public static void RuleForUpdateShareableUserList<T>(this AbstractValidator<T> validator, Expression<Func<T, List<string>>> expression)
    {
        validator.RuleFor(expression)
            .ForEach(rule => rule
                .Length(5, 20)
                .Must(userName => userName.All(c => AllowedUserNameCharacters.Contains(c)))
                .WithMessage($"Each username must only contain the following characters: {AllowedUserNameCharacters}.")
            );
    }
}
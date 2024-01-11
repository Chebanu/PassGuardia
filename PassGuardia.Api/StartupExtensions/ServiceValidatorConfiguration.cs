using FluentValidation;

using PassGuardia.Api.Validator;
using PassGuardia.Contracts.Http;

namespace PassGuardia.Api.StartupExtensions;

public static class ServiceValidatorConfiguration
{
    public static IServiceCollection AddValidatorConfiguration(this IServiceCollection services)
    {
        return services.AddScoped<IValidator<PasswordRequest>, PasswordRequestValidator>()
                        .AddScoped<IValidator<RegisterUserRequest>, RegisterUserRequestValidator>()
                        .AddScoped<IValidator<AuthenticateUserRequest>, AuthenticateUserRequestValidator>()
                        .AddScoped<IValidator<AdminUpdateUserRequest>, AdminUpdateUserRequestValidator>()
                        .AddScoped<IValidator<AuditRequest>, AuditRequestValidator>();
    }
}
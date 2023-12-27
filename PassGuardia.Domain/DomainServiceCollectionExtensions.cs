using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using PassGuardia.Domain.Commands;
using PassGuardia.Domain.Configuration;
using PassGuardia.Domain.DbContexts;

namespace PassGuardia.Domain;

public static class DomainServiceCollectionExtensions
{
    public static IServiceCollection AddDomainServices(this IServiceCollection services,
        string connectionString,
        IConfiguration jwt)
    {
        services
            .AddIdentity<IdentityUser, IdentityRole>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
            })
            .AddEntityFrameworkStores<PasswordDbContext>()
            .AddDefaultTokenProviders();

        return services
        .AddOptions()
            .Configure<JwtSettings>(jwt)
            .AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<CreatePasswordCommand>())
            .AddDbContext<PasswordDbContext>(options => options.UseNpgsql(connectionString));
    }
}
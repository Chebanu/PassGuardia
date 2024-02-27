using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Security.Claims;
using System.Text;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

using PassGuardia.Api.Constants;
using PassGuardia.Api.Filters;
using PassGuardia.Api.StartupExtensions;
using PassGuardia.Domain;
using PassGuardia.Domain.Algorithm;
using PassGuardia.Domain.Configuration;
using PassGuardia.Domain.Constants;
using PassGuardia.Domain.DbContexts;
using PassGuardia.Domain.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOptions<PassGuardiaConfig>().Bind(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "PassGuardia",
        Description = "PassGuardia",
    });

    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            []
        }
    });
});

builder.Services.AddControllers();

builder.Services.AddDbContext<PasswordDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDomainServices(
    builder.Configuration.GetConnectionString("AppDbConnection"),
    builder.Configuration.GetSection("Jwt")
);

builder.Services
    .AddAuthorizationBuilder()
    .AddPolicy(AuthorizePolicies.Admin, policy => policy.RequireClaim(ClaimTypes.Role, Roles.Admin))
    .AddPolicy(AuthorizePolicies.User, policy => policy.RequireClaim(ClaimTypes.Role, Roles.User));


AuthenticationValidator.AddAuthenticationService(builder.Services, builder.Configuration);

ServiceValidatorConfiguration.AddValidatorConfiguration(builder.Services);

builder.Services.AddScoped<IPasswordRepository, PasswordRepository>();
builder.Services.AddScoped<IEncryptor, AesEncryptor>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<RequestAuditMiddleware>();

app.MapControllers();


await app.RunAsync();

public partial class Program { }
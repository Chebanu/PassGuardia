using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

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
builder.Services.AddSwaggerConfiguration();

builder.Services.AddControllers();

builder.Services.AddDbContext<PasswordDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDomainServices(
    builder.Configuration.GetConnectionString("AppDbConnection"),
    builder.Configuration.GetSection("Jwt")
);

builder.Services
    .AddAuthorizationBuilder()
    .AddPolicy(AuthorizePolicies.User, policy => policy.RequireClaim(ClaimTypes.Role, Roles.User));


builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        var jwtConfig = builder.Configuration.GetSection("Jwt");

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtConfig["Issuer"],
            ValidAudience = jwtConfig["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig["Secret"])),
            RoleClaimType = ClaimNames.Role,
            NameClaimType = JwtRegisteredClaimNames.Name
        };
    });

builder.Services.AddTransient<AuditActionFilter>();

ServiceValidatorConfiguration.AddValidatorConfiguration(builder.Services);

builder.Services.AddScoped<IPasswordRepository, PasswordRepository>();
builder.Services.AddScoped<IEncryptor, AesEncryptor>();


builder.Services.AddMvc(options =>
{
    options.Filters.Add(new ServiceFilterAttribute(typeof(AuditActionFilter)));
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }
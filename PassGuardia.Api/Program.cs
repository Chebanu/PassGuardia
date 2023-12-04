using Microsoft.EntityFrameworkCore;

using PassGuardia.Domain.Algorithm;
using PassGuardia.Domain.Commands;
using PassGuardia.Domain.Configuration;
using PassGuardia.Domain.DbContexts;
using PassGuardia.Domain.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddOptions<PassGuardiaConfig>().Bind(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers();

builder.Services.AddDbContext<PasswordDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IRepository, Repository>();
builder.Services.AddScoped<IEncryptor, AesEncryptor>();
builder.Services.AddMediatR(c => c.RegisterServicesFromAssembly(typeof(CreatePasswordCommand).Assembly));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

await app.RunAsync();

public partial class Program { }
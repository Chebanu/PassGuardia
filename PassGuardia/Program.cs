using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Npgsql;
using PassGuardia.Domain.Commands;
using PassGuardia.Domain.DbContexts;
using PassGuardia.Domain.Queries;
using PassGuardia.Domain.Repositories;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers();

/*builder.Services.AddDbContext<PasswordDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});*/

builder.Services.AddDbContext<PasswordDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSQL:ConnectionString"));
});


builder.Services.AddScoped<IRepository, Repository>();
builder.Services.AddMediatR(c => c.RegisterServicesFromAssembly(typeof(CreatePasswordCommand).Assembly));
builder.Services.AddMediatR(c => c.RegisterServicesFromAssembly(typeof(GetPasswordByIdQuery).Assembly));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("CorsPolicy");
app.MapControllers();
// Configure the HTTP request pipeline.


app.Run();


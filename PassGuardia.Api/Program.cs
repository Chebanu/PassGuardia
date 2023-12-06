using Microsoft.EntityFrameworkCore;

using Npgsql;

using PassGuardia.Domain.Algorithm;
using PassGuardia.Domain.Commands;
using PassGuardia.Domain.Configuration;
using PassGuardia.Domain.DbContexts;
using PassGuardia.Domain.Repositories;

using Serilog;

var builder = WebApplication.CreateBuilder(args);

using (var connection = new NpgsqlConnection(builder.Configuration.GetConnectionString("DefaultConnection")))
{
    connection.Open();
    using (var command = new NpgsqlCommand())
    {
        command.Connection = connection;
        command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Audit (
                    Id SERIAL PRIMARY KEY,
                    RequestPath TEXT,
                    RequestMethod TEXT,
                    ResponseStatusCode INT,
                    Timestamp TIMESTAMPTZ
                );";
        command.ExecuteNonQuery();
    }
}

builder.Host.UseSerilog((context, config) =>
{
    var connectionString = context.Configuration.GetConnectionString("DefaultConnection");

    config.WriteTo.PostgreSQL(connectionString, "Audit", needAutoCreateTable: true).MinimumLevel.Information();

    if (!context.HostingEnvironment.IsProduction())
    {
        config.WriteTo.Console().MinimumLevel.Information();
    }
});

builder.Services.AddOptions<PassGuardiaConfig>().Bind(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers( options =>
{
    options.Filters.Add<AuditActionFilter>();
});

builder.Services.AddDbContext<PasswordDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<AuditActionFilter>();
builder.Services.AddScoped<IRepository, Repository>();
builder.Services.AddScoped<IEncryptor, AesEncryptor>();
builder.Services.AddMediatR(c => c.RegisterServicesFromAssembly(typeof(CreatePasswordCommand).Assembly));

var app = builder.Build();

app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

await app.RunAsync();

public partial class Program { }
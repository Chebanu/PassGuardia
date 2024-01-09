using System.Text.Json;

using PassGuardia.Contracts.Models;
using PassGuardia.Domain.DbContexts;

namespace PassGuardia.Api.Filters;

public class RequestAuditMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IServiceScopeFactory _scopeFactory;

    public RequestAuditMiddleware(RequestDelegate next, IServiceScopeFactory scopeFactory)
    {
        _next = next;
        _scopeFactory = scopeFactory;
    }

    public async Task Invoke(HttpContext context)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PasswordDbContext>();
        var audit = new Audit
        {
            RequestPath = context.Request.Path,
            RequestMethod = context.Request.Method,
            Timestamp = DateTime.UtcNow
        };

        try
        {
            await _next(context);
            audit.StatusCode = context.Response.StatusCode;
        }
        catch (Exception ex)
        {
            audit.StatusCode = context.Response.StatusCode;
            audit.Exception = JsonSerializer.Serialize(new
            {
                ex.Message,
                ex.StackTrace
            });
        }

        await dbContext.Audits.AddAsync(audit);
        await dbContext.SaveChangesAsync();
    }
}
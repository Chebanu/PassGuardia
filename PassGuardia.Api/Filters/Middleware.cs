using System.Text.Json;

using PassGuardia.Contracts.Models;
using PassGuardia.Domain.DbContexts;

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
        using (var scope = _scopeFactory.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<PasswordDbContext>();
            var audit = new Audit();

            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                audit.Exception = JsonSerializer.Serialize(new
                {
                    ex.Message,
                    ex.StackTrace
                });
            }
            finally
            {
                audit.RequestPath = context.Request.Path;
                audit.RequestMethod = context.Request.Method;
                audit.Timestamp = DateTime.UtcNow;
                audit.StatusCode = context.Response.StatusCode;

                await dbContext.Audits.AddAsync(audit);
                await dbContext.SaveChangesAsync();
            }
        }
    }
}

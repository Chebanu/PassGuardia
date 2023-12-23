using System.Text.Json;

using Microsoft.AspNetCore.Mvc.Filters;

using PassGuardia.Contracts.Models;
using PassGuardia.Domain.Repositories;

namespace PassGuardia.Api.Filters;

public class AuditActionFilter : IAsyncActionFilter
{
    private readonly IPasswordRepository _repository;

    public AuditActionFilter(IPasswordRepository repository)
    {
        _repository = repository;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var audit = new Audit()
        {
            RequestPath = context.HttpContext.Request.Path,
            RequestMethod = context.HttpContext.Request.Method,
            Timestamp = DateTime.UtcNow
        };

        var result = await next();
        if (result.Exception != null)
        {
            audit.Exception = JsonSerializer.Serialize(new
            {
                result.Exception.Message,
                result.Exception.StackTrace
            });
        }

        // ToDo: known issue, status code always 200
        audit.StatusCode = result.HttpContext.Response.StatusCode;

        await _repository.CreateAudit(audit);
    }
}
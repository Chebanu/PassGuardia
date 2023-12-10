using Microsoft.AspNetCore.Mvc.Filters;

using PassGuardia.Contracts.Models;
using PassGuardia.Domain.Repositories;

namespace PassGuardia.Api.Filters;

public class AuditActionFilter : IAsyncResultFilter
{
    private readonly IRepository _repository;

    public AuditActionFilter(IRepository repository)
    {
        _repository = repository;
    }

    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
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
            audit.Exception = GetExceptionDetails(result.Exception);
        }

        audit.StatusCode = result.HttpContext.Response.StatusCode;

        await _repository.CreateAudit(audit);
    }

    private static string GetExceptionDetails(Exception exception)
    {
        return exception == null
            ? null
            : $"{exception.GetType().FullName}: {exception.Message}\nStackTrace: {exception.StackTrace}";
    }
}
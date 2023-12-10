using Microsoft.AspNetCore.Mvc.Filters;

using PassGuardia.Contracts.Models;
using PassGuardia.Domain.Repositories;

public class AuditActionFilter : ActionFilterAttribute
{
    private readonly IRepository _repository;

    public AuditActionFilter(IRepository repository)
    {
        _repository = repository;
    }

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var audit = new Audit()
        {
            RequestPath = context.HttpContext.Request.Path,
            RequestMethod = context.HttpContext.Request.Method,
            Timestamp = DateTime.UtcNow
        };

        var result = await next();

        if (result != null)
        {
            audit.Exception = GetExceptionDetails(result.Exception);
        }

        audit.StatusCode = result.HttpContext.Response.StatusCode;

        await _repository.CreateAudit(audit);
    }

    private string GetExceptionDetails(Exception exception)
    {
        if (exception == null)
        {
            return null;
        }

        return $"{exception.GetType().FullName}: {exception.Message}\nStackTrace: {exception.StackTrace}";
    }
}
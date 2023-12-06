using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc.Filters;

public class AuditActionFilter : ActionFilterAttribute
{
    private readonly ILogger<AuditActionFilter> _logger;

    public AuditActionFilter(ILogger<AuditActionFilter> logger)
    {
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        LogAction(context, "Executing");
    }

    public override void OnActionExecuted(ActionExecutedContext context)
    {
        LogAction(context, "Executed");
    }

    private void LogAction(FilterContext context, string actionType)
    {
        var requestPath = context.HttpContext.Request.Path;
        var controller = context.RouteData.Values["controller"];
        var action = context.RouteData.Values["action"];
        var httpMethod = context.HttpContext.Request;
        var statusCode = context.HttpContext.Response.StatusCode;
        var date = DateTime.UtcNow;
        var exceptionFeature = context.HttpContext.Features.Get<IExceptionHandlerFeature>();
        var exception = exceptionFeature?.Error;

        if (statusCode >= 400)
        {
            _logger.LogError($"{exception}\nAudit - {actionType}: {controller}.{action}.{statusCode}",
                               $"{requestPath.Value}",
                               $"{httpMethod.Method}",
                               statusCode,
                               date);
        }
        else
        {
            _logger.LogInformation($"Audit - {actionType}: {controller}.{action}.{statusCode}",
                                  $"{requestPath.Value}",
                                  $"{httpMethod.Method}",
                                  statusCode,
                                  date);
        }
    }
}

using MediatR;

using Microsoft.Extensions.Logging;

namespace PassGuardia.Domain.Base;

public abstract class BaseRequestHandler<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<BaseRequestHandler<TRequest, TResponse>> _logger;

    public BaseRequestHandler(ILogger<BaseRequestHandler<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug($"Handling {GetType().Name}. Request: {request}");

            TResponse result = await HandleInternal(request, cancellationToken);

            _logger.LogInformation($"Handled {GetType().Name}. Request: {request}. Result: {result}");
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error handling {GetType().Name}. Request: {request}");
            throw;
        }
    }

    protected abstract Task<TResponse> HandleInternal(TRequest request, CancellationToken cancellationToken);
}
using MediatR;

namespace PassGuardia.Domain.Base;

public abstract class BaseRequestHandler<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public BaseRequestHandler()
    {
    }

    public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken)
    {
        try
        {
            TResponse result = await HandleInternal(request, cancellationToken);
            return result;
        }
        catch
        {
            throw;
        }
    }

    protected abstract Task<TResponse> HandleInternal(TRequest request, CancellationToken cancellationToken);
}
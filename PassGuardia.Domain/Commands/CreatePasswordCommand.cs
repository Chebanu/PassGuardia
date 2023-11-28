using MediatR;

using Microsoft.Extensions.Options;

using PassGuardia.Domain.Algorithm;
using PassGuardia.Domain.Base;
using PassGuardia.Domain.Configuration;
using PassGuardia.Domain.Repositories;

namespace PassGuardia.Domain.Commands;

public class CreatePasswordCommand : IRequest<CreatePasswordResult>
{
    public string Password { get; init; }
}
public class CreatePasswordResult
{
    public Guid Id { get; init; }
}

internal class CreatePasswordCommandHandler : BaseRequestHandler<CreatePasswordCommand, CreatePasswordResult>
{
    private readonly IRepository _repository;
    private readonly IOptionsMonitor<PassGuardiaConfig> _options;

    public CreatePasswordCommandHandler(IRepository repository, IOptionsMonitor<PassGuardiaConfig> options)
    {
        _repository = repository;
        _options = options;
    }

    protected override async Task<CreatePasswordResult> HandleInternal(CreatePasswordCommand request, CancellationToken cancellationToken)
    {
        var password = SymmentricAlgorithm.CreatePassword(request.Password);

        await _repository.CreatePassword(password, cancellationToken);

        return new CreatePasswordResult
        {
            Id = password.Id
        };
    }
}
using MediatR;
using PassGuardia.Domain.Algorithm;
using PassGuardia.Domain.Base;
using PassGuardia.Domain.Repositories;
using System.Diagnostics;

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

    public CreatePasswordCommandHandler(IRepository repository)
    {
        _repository = repository;
    }

    protected override async Task<CreatePasswordResult> HandleInternal(CreatePasswordCommand request, CancellationToken cancellationToken)
    {
       var password = SymmentricAlgorithm.CreatePassword(request.Password);

        _ = await _repository.CreatePassword(password.EncryptedPassword, password.IV, default);

        return new CreatePasswordResult
        {
            Id = password.Id
        };
    }
}

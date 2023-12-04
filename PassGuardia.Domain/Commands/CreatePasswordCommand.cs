using MediatR;

using Microsoft.Extensions.Options;

using PassGuardia.Contracts.Models;
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
public class CreatePasswordCommandHandler : BaseRequestHandler<CreatePasswordCommand, CreatePasswordResult>
{
    private readonly IRepository _repository;
    private readonly IEncryptor _encryptor;
    private readonly IOptionsMonitor<PassGuardiaConfig> _options;

    public CreatePasswordCommandHandler(IRepository repository, IEncryptor encryptor, IOptionsMonitor<PassGuardiaConfig> options)
    {
        _repository = repository;
        _encryptor = encryptor;
        _options = options;
    }

    protected override async Task<CreatePasswordResult> HandleInternal(CreatePasswordCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ArgumentException(nameof(request.Password));
        }

        if (request.Password.Length < 1 || request.Password.Length > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(request.Password));
        }

        var keyConfig = _options.CurrentValue;

        var passwordAlgorithm = _encryptor.Encrypt(request.Password, keyConfig.EncryptionKey);

        var password = new Password
        {
            Id = Guid.NewGuid(),
            EncryptedPassword = passwordAlgorithm
        };

        _ = await _repository.CreatePassword(password, cancellationToken);

        return new CreatePasswordResult
        {
            Id = password.Id
        };
    }
}
using MediatR;

using Microsoft.Extensions.Options;

using PassGuardia.Domain.Algorithm;
using PassGuardia.Domain.Handlers;
using PassGuardia.Domain.Configuration;
using PassGuardia.Domain.Repositories;

namespace PassGuardia.Domain.Queries;

public class GetPasswordByIdQuery : IRequest<GetPasswordByIdResult>
{
    public Guid Id { get; init; }
}

public class GetPasswordByIdResult
{
    public string Password { get; init; }
}

public class GetPasswordByIdQueryHandler : BaseRequestHandler<GetPasswordByIdQuery, GetPasswordByIdResult>
{
    private readonly IPasswordRepository _repository;
    private readonly IEncryptor _encryptor;
    private readonly IOptionsMonitor<PassGuardiaConfig> _options;

    public GetPasswordByIdQueryHandler(IPasswordRepository repository, IEncryptor encryptor, IOptionsMonitor<PassGuardiaConfig> options)
    {
        _repository = repository;
        _encryptor = encryptor;
        _options = options;
    }

    protected override async Task<GetPasswordByIdResult> HandleInternal(GetPasswordByIdQuery request, CancellationToken cancellationToken)
    {
        var keyConfig = _options.CurrentValue;
        var dbPassword = await _repository.GetPasswordById(request.Id, cancellationToken);

        if (dbPassword == null) return new GetPasswordByIdResult { Password = null };

        var password = _encryptor.Decrypt(dbPassword.EncryptedPassword, keyConfig.EncryptionKey);

        return new GetPasswordByIdResult { Password = password };
    }
}
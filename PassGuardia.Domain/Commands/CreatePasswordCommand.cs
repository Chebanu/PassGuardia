﻿using MediatR;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using PassGuardia.Contracts.Models;
using PassGuardia.Domain.Algorithm;
using PassGuardia.Domain.Configuration;
using PassGuardia.Domain.Handlers;
using PassGuardia.Domain.Repositories;

namespace PassGuardia.Domain.Commands;

public class CreatePasswordCommand : IRequest<CreatePasswordResult>
{
    public string User { get; init; }
    public string Password { get; init; }
    public bool IsPrivate { get; init; }
}
public class CreatePasswordResult
{
    public Guid PasswordId { get; init; }
    public string[] Errors { get; set; }
}
public class CreatePasswordCommandHandler : BaseRequestHandler<CreatePasswordCommand, CreatePasswordResult>
{
    private readonly IPasswordRepository _repository;
    private readonly IEncryptor _encryptor;
    private readonly IOptionsMonitor<PassGuardiaConfig> _options;

    public CreatePasswordCommandHandler(IPasswordRepository repository,
                                        IEncryptor encryptor, IOptionsMonitor<PassGuardiaConfig> options,
                                        ILogger<BaseRequestHandler<CreatePasswordCommand, CreatePasswordResult>> logger) : base(logger)
    {
        _repository = repository;
        _encryptor = encryptor;
        _options = options;
    }

    protected override async Task<CreatePasswordResult> HandleInternal(CreatePasswordCommand request, CancellationToken cancellationToken)
    {
        var keyConfig = _options.CurrentValue;

        var passwordAlgorithm = _encryptor.Encrypt(request.Password, keyConfig.EncryptionKey);

        var password = new Password
        {
            Id = Guid.NewGuid(),
            EncryptedPassword = passwordAlgorithm,
            CreatedBy = request.User,
            IsPrivate = request.IsPrivate
        };

        _ = await _repository.CreatePassword(password, cancellationToken);

        return new CreatePasswordResult
        {
            PasswordId = password.Id
        };
    }
}
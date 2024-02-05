using MediatR;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using PassGuardia.Contracts.Http;
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
    public Visibility Visibility { get; init; }
    public List<string> ShareableList { get; init; }
}
public class CreatePasswordResult
{
    public Guid PasswordId { get; init; }
    public string[] Errors { get; init; }
    public bool Success { get; init; }
}
public class CreatePasswordCommandHandler : BaseRequestHandler<CreatePasswordCommand, CreatePasswordResult>
{
    private readonly IPasswordRepository _repository;
    private readonly IEncryptor _encryptor;
    private readonly IOptionsMonitor<PassGuardiaConfig> _options;
    private readonly UserManager<IdentityUser> _userManager;

    public CreatePasswordCommandHandler(IPasswordRepository repository,
                                        IEncryptor encryptor, IOptionsMonitor<PassGuardiaConfig> options,
                                        UserManager<IdentityUser> userManager,
                                        ILogger<BaseRequestHandler<CreatePasswordCommand, CreatePasswordResult>> logger) : base(logger)
    {
        _repository = repository;
        _encryptor = encryptor;
        _options = options;
        _userManager = userManager;
    }

    protected override async Task<CreatePasswordResult> HandleInternal(CreatePasswordCommand request, CancellationToken cancellationToken)
    {
        if (request.ShareableList != null)
        {
            if (request.ShareableList.Contains(request.User))
            {
                return new CreatePasswordResult
                {
                    Success = false,
                    Errors = ["Owner cannot be in the shared list"]

                };
            }

            var nonExistentUsernames = new List<string>();

            foreach (var username in request.ShareableList)
            {
                var user = await _userManager.FindByNameAsync(username);

                if (user == null)
                {
                    nonExistentUsernames.Add(username);
                }
            }

            if (nonExistentUsernames.Any())
            {
                return new CreatePasswordResult
                {
                    Success = false,
                    Errors = [$"Non-existent users: {string.Join(", ", nonExistentUsernames)}"]
                };
            }

            var duplicates = request.ShareableList
                                .GroupBy(s => s)
                                .Where(g => g.Count() > 1)
                                .SelectMany(g => g)
                                .ToList();

            if (duplicates.Count > 0)
            {
                return new CreatePasswordResult
                {
                    Success = false,
                    Errors = [$"Duplicate usernames: {string.Join(", ", duplicates.Distinct())}, {duplicates.Count} times"]
                };
            }
        }

        var passwordAlgorithm = _encryptor.Encrypt(request.Password, _options.CurrentValue.EncryptionKey);

        var password = new Password
        {
            Id = Guid.NewGuid(),
            EncryptedPassword = passwordAlgorithm,
            CreatedBy = request.User,
            Visibility = request.Visibility,
            ShareableList = request.ShareableList
        };

        _ = await _repository.CreatePassword(password, cancellationToken);

        return new CreatePasswordResult
        {
            Success = true,
            PasswordId = password.Id
        };
    }
}
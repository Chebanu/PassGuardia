using MediatR;

using Microsoft.Extensions.Logging;

using PassGuardia.Contracts.Http;
using PassGuardia.Contracts.Models;
using PassGuardia.Domain.Handlers;
using PassGuardia.Domain.Repositories;

namespace PassGuardia.Domain.Commands;

public class UpdatePasswordVisibilityCommand : IRequest<UpdatePasswordVisibilityResult>
{
    public required UpdatePasswordVisibilityRequest PasswordVisibility { get; init; }
    public string User { get; init; }
}

public class UpdatePasswordVisibilityResult
{
    public Visibility GetVisibility { get; init; }
    public bool Success { get; init; }
    public IEnumerable<string> Errors { get; init; }
}

internal class UpdatePasswordVisibilityCommandHandler : BaseRequestHandler<UpdatePasswordVisibilityCommand, UpdatePasswordVisibilityResult>
{
    private readonly IPasswordRepository _repository;

    public UpdatePasswordVisibilityCommandHandler(IPasswordRepository repository,
                                        ILogger<UpdatePasswordVisibilityCommandHandler> logger) : base(logger)
    {
        _repository = repository;
    }

    protected override async Task<UpdatePasswordVisibilityResult> HandleInternal(UpdatePasswordVisibilityCommand request,
                                                                            CancellationToken cancellationToken)
    {
        var dbPassword = await _repository.GetPasswordById(request.PasswordVisibility.Id, cancellationToken);

        if (dbPassword == null)
        {
            return new UpdatePasswordVisibilityResult
            {
                Success = false,
                Errors = ["Password doesn't exist"]
            };
        }

        if (dbPassword.CreatedBy != request.User)
        {
            return new UpdatePasswordVisibilityResult
            {
                Success = false,
                Errors = ["Access denied to other users"]
            };
        }

        if (dbPassword.GetVisibility == request.PasswordVisibility.GetVisibility)
        {
            return new UpdatePasswordVisibilityResult
            {
                Success = false,
                Errors = ["Visibility is the same as previous one"]
            };
        }

        dbPassword.GetVisibility = request.PasswordVisibility.GetVisibility;

        await _repository.UpdatePasswordVisibility(dbPassword);

        return new UpdatePasswordVisibilityResult
        {
            Success = true,
            GetVisibility = dbPassword.GetVisibility
        };
    }
}

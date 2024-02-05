using MediatR;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

using PassGuardia.Contracts.Http;
using PassGuardia.Contracts.Models;
using PassGuardia.Domain.Handlers;
using PassGuardia.Domain.Repositories;

namespace PassGuardia.Domain.Commands;

public class UpdatePasswordCommand : IRequest<UpdatePasswordResult>
{
    public required Guid Id { get; init; }
    public required UpdatePasswordRequest Password { get; init; }
    public string User { get; init; }
}

public class UpdatePasswordResult
{
    public Visibility Visibility { get; init; }
    public bool Success { get; init; }
    public IEnumerable<string> Errors { get; set; }
}

internal class UpdatePasswordVisibilityCommandHandler : BaseRequestHandler<UpdatePasswordCommand, UpdatePasswordResult>
{
    private readonly IPasswordRepository _repository;
    private readonly UserManager<IdentityUser> _userManager;

    public UpdatePasswordVisibilityCommandHandler(IPasswordRepository repository,
                                                UserManager<IdentityUser> userManager,
                                                ILogger<UpdatePasswordVisibilityCommandHandler> logger) : base(logger)
    {
        _repository = repository;
        _userManager = userManager;
    }

    protected override async Task<UpdatePasswordResult> HandleInternal(UpdatePasswordCommand request,
                                                                    CancellationToken cancellationToken)
    {
        var dbPassword = await _repository.GetPasswordById(request.Id, cancellationToken);

        if (dbPassword == null)
        {
            return new UpdatePasswordResult
            {
                Success = false,
                Errors = ["Password doesn't exist"]
            };
        }

        if (dbPassword.CreatedBy != request.User)
        {
            return new UpdatePasswordResult
            {
                Success = false,
                Errors = ["Access denied to other users"]
            };
        }

        if ((dbPassword.Visibility == Visibility.Shared && request.Password.Visibility != Visibility.Shared) ||
            (dbPassword.Visibility != Visibility.Shared && request.Password.Visibility == Visibility.Shared))
        {
            return new UpdatePasswordResult
            {
                Success = false,
                Errors = ["You are not able to change 'Shared' Visibility"]
            };
        }

        if (request.Password.ShareableList == null && dbPassword.Visibility == Visibility.Shared)
        {
            return new UpdatePasswordResult
            {
                Success = false,
                Errors = ["Shareable list is null"]
            };
        }

        var isSharedPasswordValid = dbPassword.Visibility == Visibility.Shared && request.Password.Visibility == Visibility.Shared;

        if (isSharedPasswordValid)
        {
            if (dbPassword.ShareableList.Count == request.Password.ShareableList.Count &&
                                         dbPassword.ShareableList.All(user => request.Password.ShareableList.Contains(user)))
            {
                return new UpdatePasswordResult
                {
                    Success = false,
                    Errors = ["You didn't apply any changes"]
                };
            }
        }

        if (isSharedPasswordValid)
        {
            if (request.Password.ShareableList.Contains(request.User))
            {
                return new UpdatePasswordResult
                {
                    Success = false,
                    Errors = ["Owner cannot be in the shared list"]

                };
            }

            var nonExistentUsernames = new List<string>();

            foreach (var username in request.Password.ShareableList)
            {
                var user = await _userManager.FindByNameAsync(username);

                if (user == null)
                {
                    nonExistentUsernames.Add(username);
                }
            }

            if (nonExistentUsernames.Any())
            {
                return new UpdatePasswordResult
                {
                    Success = false,
                    Errors = [$"Non-existent users: {string.Join(", ", nonExistentUsernames)}"]
                };
            }

            var duplicates = request.Password.ShareableList
                                                .GroupBy(s => s)
                                                .Where(g => g.Count() > 1)
                                                .SelectMany(g => g)
                                                .ToList();

            if (duplicates.Count > 0)
            {
                return new UpdatePasswordResult
                {
                    Success = false,
                    Errors = [$"Duplicate usernames: {string.Join(", ", duplicates.Distinct())}, {duplicates.Count} times"]
                };
            }

            dbPassword.ShareableList = request.Password?.ShareableList;
        }
        else
        {
            if (request.Password.ShareableList?.Count > 0)
            {
                return new UpdatePasswordResult
                {
                    Success = false,
                    Errors = ["You cannot apply shared list to non-shareable list"]
                };
            }

            if (dbPassword.Visibility != request.Password.Visibility)
            {
                dbPassword.Visibility = request.Password.Visibility;
            }
            else
            {
                return new UpdatePasswordResult
                {
                    Success = false,
                    Errors = ["Visibility is the same as the previous one"]
                };
            }
        }

        await _repository.UpdatePasswordVisibility(dbPassword);

        return new UpdatePasswordResult
        {
            Success = true,
            Visibility = dbPassword.Visibility
        };
    }
}
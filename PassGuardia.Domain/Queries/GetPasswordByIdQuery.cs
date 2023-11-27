using MediatR;
using PassGuardia.Domain.Algorithm;
using PassGuardia.Domain.Base;
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
    private readonly IRepository _repository;

    public GetPasswordByIdQueryHandler(IRepository repository)
    {
        _repository = repository;
    }

    protected override async Task<GetPasswordByIdResult> HandleInternal(GetPasswordByIdQuery request, CancellationToken cancellationToken)
    {
        var dbPassword = await _repository.GetPasswordById(request.Id, cancellationToken);

        var password = SymmentricAlgorithm.Decrypt(dbPassword.EncryptedPassword, dbPassword.IV);

        return new GetPasswordByIdResult { Password = password };
    }
}
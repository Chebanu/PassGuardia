using MediatR;

using Microsoft.Extensions.Logging;

using PassGuardia.Contracts.Models;
using PassGuardia.Domain.Handlers;
using PassGuardia.Domain.Repositories;

namespace PassGuardia.Domain.Queries;

public class GetAuditLogQuery : IRequest<GetAuditLogResult>
{
    public int PageSize { get; init; }
    public int PageNumber { get; init; }
}

public class GetAuditLogResult
{
    public List<Audit> Audits { get; init; }
}

public class GetAuditLogQueryHandler : BaseRequestHandler<GetAuditLogQuery, GetAuditLogResult>
{
    private readonly IPasswordRepository _repository;

    public GetAuditLogQueryHandler(IPasswordRepository repository, ILogger<GetAuditLogQueryHandler> logger) : base(logger)
    {
        _repository = repository;
    }

    protected override async Task<GetAuditLogResult> HandleInternal(GetAuditLogQuery request, CancellationToken cancellationToken)
    {
        var audits = await _repository.GetAudits(request.PageNumber, request.PageSize, cancellationToken);
        return new GetAuditLogResult { Audits = audits };
    }
}
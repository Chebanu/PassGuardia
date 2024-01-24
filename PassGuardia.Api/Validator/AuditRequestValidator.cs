using FluentValidation;

using PassGuardia.Contracts.Http;

namespace PassGuardia.Api.Validator;

public class AuditRequestValidator : AbstractValidator<AuditRequest>
{
    public AuditRequestValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100);
    }
}
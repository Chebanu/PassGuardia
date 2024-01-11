using FluentValidation;

using PassGuardia.Contracts.Http;

namespace PassGuardia.Api.Validator;

public class AuditRequestValidator : AbstractValidator<AuditRequest>
{
    public AuditRequestValidator()
    {
        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page number should be greater than 0");

        RuleFor(x => x.PageNumber)
            .InclusiveBetween(1, 100)
            .WithMessage("Size number must be between 1 and 100.");
    }
}
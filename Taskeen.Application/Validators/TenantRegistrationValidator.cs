using FluentValidation;
using Taskeen.Application.DTOs;

namespace Taskeen.Application.Validators;

public class TenantRegistrationValidator : AbstractValidator<TenantRegistrationDTO>
{
    public TenantRegistrationValidator()
    {
        RuleFor(x => x.IdentityNumber).NotEmpty().MaximumLength(20);
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Nationality).NotEmpty().MaximumLength(50);
    }
}

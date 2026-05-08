using FluentValidation;
using Taskeen.Application.DTOs;

namespace Taskeen.Application.Validators;

public class OwnerCalendarValidator : AbstractValidator<OwnerCalendarDTO>
{
    public OwnerCalendarValidator()
    {
        RuleFor(x => x.UnitCode).NotEmpty();
        RuleFor(x => x.StartDate).NotEmpty();
        RuleFor(x => x.EndDate).NotEmpty().GreaterThan(x => x.StartDate)
            .WithMessage("End date must be after start date.");
    }
}

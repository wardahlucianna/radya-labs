using BinusSchool.Data.Model.Scheduling.FnSchedule.EventType;
using FluentValidation;

namespace BinusSchool.Scheduling.FnSchedule.EventType.Validator
{
    public class AddEventTypeValidator : AbstractValidator<AddEventTypeRequest>
    {
        public AddEventTypeValidator()
        {
            RuleFor(x => x.IdAcademicYear).NotEmpty();

            RuleFor(x => x.Color).NotEmpty();

            RuleFor(x => x.Description)
                .NotEmpty()
                .WithName("Event Type Name");
        }
    }
}

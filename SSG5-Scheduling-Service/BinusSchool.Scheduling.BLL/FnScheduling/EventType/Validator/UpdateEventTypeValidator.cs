using BinusSchool.Data.Model.Scheduling.FnSchedule.EventType;
using FluentValidation;

namespace BinusSchool.Scheduling.FnSchedule.EventType.Validator
{
    public class UpdateEventTypeValidator : AbstractValidator<UpdateEventTypeRequest>
    {
        public UpdateEventTypeValidator()
        {
            Include(new AddEventTypeValidator());

            RuleFor(x => x.Id).NotEmpty();
        }
    }
}

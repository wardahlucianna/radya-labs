using BinusSchool.Data.Model.Scheduling.FnSchedule.Schedule;
using FluentValidation;

namespace BinusSchool.Scheduling.FnSchedule.Schedule.Validator
{
    public class UpdateScheduleValidator : AbstractValidator<UpdateScheduleRequest>
    {
        public UpdateScheduleValidator()
        {
            RuleFor(x => x.IdSchedule).NotEmpty();

            RuleFor(x => x.IdSession).NotEmpty();

            RuleFor(x => x.IdDay).NotEmpty();

            RuleFor(x => x.IdLesson).NotEmpty();

            RuleFor(x => x.IdUser).NotEmpty();

            RuleFor(x => x.IdVenue).NotEmpty();

            RuleFor(x => x.IdWeekVarianDetail).NotEmpty();
        }
    }
}

using BinusSchool.Data.Model.Scheduling.FnSchedule.Schedule;
using FluentValidation;

namespace BinusSchool.Scheduling.FnSchedule.Schedule.Validator
{
    public class AddScheduleValidator : AbstractValidator<AddScheduleRequest>
    {
        public AddScheduleValidator()
        {
            RuleFor(x => x.IdAscTimeTable).NotEmpty();

            RuleFor(x => x.IdGrade).NotEmpty();

            RuleFor(x => x.IdSession).NotEmpty();

            RuleFor(x => x.IdDay).NotEmpty();

            RuleFor(x => x.Schedules)
                .NotNull();

            RuleForEach(x => x.Schedules).ChildRules(schedules => {          

                schedules.RuleFor(x => x.IdLesson).NotEmpty();

                schedules.RuleFor(x => x.IdUser).NotEmpty();

                schedules.RuleFor(x => x.IdVenue).NotEmpty();

                schedules.RuleFor(x => x.IdWeekVarianDetail).NotEmpty();

            });
        }
    }
}

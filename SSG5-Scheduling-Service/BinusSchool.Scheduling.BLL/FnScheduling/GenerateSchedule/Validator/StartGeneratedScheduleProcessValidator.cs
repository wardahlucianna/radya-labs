using BinusSchool.Data.Model.Scheduling.FnSchedule.GenerateSchedule;
using FluentValidation;

namespace BinusSchool.Scheduling.FnSchedule.GeneratedSchedule.Validator
{
    public class StartGeneratedScheduleProcessValidator : AbstractValidator<StartGeneratedScheduleProcessRequest>
    {
        public StartGeneratedScheduleProcessValidator()
        {
            RuleFor(x => x.IdSchool).NotEmpty();

            RuleFor(x => x.Grades).NotEmpty();
            RuleFor(x => x.Version).NotEmpty();
        }
    }
}

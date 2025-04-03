using BinusSchool.Data.Model.Scheduling.FnAscTimetable.AscTimeTables;
using FluentValidation;

namespace BinusSchool.Scheduling.FnAscTimetable.AscTimetable.Validator
{
    public class StartAscTimetableProcessValidator : AbstractValidator<StartAscTimetableProcessRequest>
    {
        public StartAscTimetableProcessValidator()
        {
            RuleFor(x => x.IdSchool).NotEmpty();

            RuleFor(x => x.Grades).NotEmpty();
        }
    }
}

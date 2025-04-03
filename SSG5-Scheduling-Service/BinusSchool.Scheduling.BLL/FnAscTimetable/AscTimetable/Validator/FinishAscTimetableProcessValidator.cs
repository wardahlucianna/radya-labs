using BinusSchool.Data.Model.Scheduling.FnAscTimetable.AscTimeTables;
using FluentValidation;

namespace BinusSchool.Scheduling.FnAscTimetable.AscTimetable.Validator
{
    public class FinishAscTimetableProcessValidator : AbstractValidator<FinishAscTimetableProcessRequest>
    {
        public FinishAscTimetableProcessValidator()
        {
            RuleFor(x => x.IdProcess).NotEmpty();
        }
    }
}

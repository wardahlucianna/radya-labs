using BinusSchool.Data.Model.Attendance.FnAttendance.Attendance;
using FluentValidation;

namespace BinusSchool.Attendance.FnAttendance.EventAttendanceEntry.Validator
{
    public class ChooseConflictedEventValidator : AbstractValidator<ChooseConflictedEventRequest>
    {
        public ChooseConflictedEventValidator()
        {
            RuleFor(x => x.ConflictCode)
                .NotEmpty()
                .Must(x => x.Split('_').Length >= 2);

            RuleFor(x => x.IdEventCheck)
                .NotEmpty();
        }
    }
}

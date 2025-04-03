using BinusSchool.Data.Model.Attendance.FnAttendance.Formula;
using FluentValidation;

namespace BinusSchool.Attendance.FnAttendance.Formula.Validator
{
    public class SetFormulaValidator : AbstractValidator<SetFormulaRequest>
    {
        public SetFormulaValidator()
        {
            RuleFor(x => x.IdLevel).NotNull();
            RuleFor(x => x.AttendanceRate).NotNull();
            RuleFor(x => x.PresenceInClass).NotNull();
        }
    }
}

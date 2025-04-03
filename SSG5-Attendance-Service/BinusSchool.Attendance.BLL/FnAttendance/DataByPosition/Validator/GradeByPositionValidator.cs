using BinusSchool.Data.Model.Attendance.FnAttendance.GradeByPosition;
using FluentValidation;

namespace BinusSchool.Attendance.FnAttendance.DataByPosition.Validator
{
    public class GradeByPositionValidator : AbstractValidator<GradeByPositionRequest>
    {
        public GradeByPositionValidator()
        {
            RuleFor(x => x.IdAcademicYear).NotEmpty();
            RuleFor(x => x.IdLevel).NotEmpty();
            RuleFor(x => x.IdUser).NotEmpty();
            RuleFor(x => x.SelectedPosition).NotEmpty();
        }
    }
}

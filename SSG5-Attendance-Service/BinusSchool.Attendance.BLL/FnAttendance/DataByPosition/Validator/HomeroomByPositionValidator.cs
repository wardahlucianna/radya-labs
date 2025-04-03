using BinusSchool.Data.Model.Attendance.FnAttendance.HomeroomByPosition;
using FluentValidation;

namespace BinusSchool.Attendance.FnAttendance.DataByPosition.Validator
{
    public class HomeroomByPositionValidator : AbstractValidator<HomeroomByPositionRequest>
    {
        public HomeroomByPositionValidator()
        {
            RuleFor(x => x.IdAcademicYear).NotEmpty();
            RuleFor(x => x.IdLevel).NotEmpty();
            RuleFor(x => x.IdGrade).NotEmpty();
            RuleFor(x => x.IdUser).NotEmpty();
            RuleFor(x => x.SelectedPosition).NotEmpty();
            RuleFor(x => x.Semester).NotEmpty();
        }
    }
}

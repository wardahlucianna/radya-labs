using BinusSchool.Data.Model.Attendance.FnAttendance.LevelByPosition;
using FluentValidation;

namespace BinusSchool.Attendance.FnAttendance.Validator
{
    public class LevelByPositionValidator : AbstractValidator<LevelByPositionRequest>
    {
        public LevelByPositionValidator()
        {
            RuleFor(x => x.IdAcademicYear).NotEmpty();
            RuleFor(x => x.IdUser).NotEmpty();
            RuleFor(x => x.SelectedPosition).NotEmpty();
        }
    }
}

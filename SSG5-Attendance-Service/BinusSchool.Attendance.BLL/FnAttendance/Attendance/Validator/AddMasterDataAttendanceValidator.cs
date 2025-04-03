using BinusSchool.Data.Model.Attendance.FnAttendance.Attendance;
using FluentValidation;

namespace BinusSchool.Attendance.FnAttendance.Attendance.Validator
{
    public class AddMasterDataAttendanceValidator : AbstractValidator<AddMasterDataAttendanceRequest>
    {
        public AddMasterDataAttendanceValidator()
        {
            RuleFor(x => x.IdAcademicYear).NotEmpty();

            RuleFor(x => x.AttendanceName).NotEmpty();

            RuleFor(x => x.ShortName).NotEmpty();

            RuleFor(x => x.AttendanceCategory).NotEmpty();

            RuleFor(x => x.Status).NotNull();

            RuleFor(x => x.IsNeedFileAttachment).NotNull();

        }
    }
}

using BinusSchool.Data.Model.Attendance.FnAttendance.Attendance;
using FluentValidation;

namespace BinusSchool.Attendance.FnAttendance.Attendance.Validator
{
    public class UpdateMasterDataAttendanceValidator : AbstractValidator<UpdateMasterDataAttendanceRequest>
    {
        public UpdateMasterDataAttendanceValidator()
        {
            Include(new AddMasterDataAttendanceValidator());

            RuleFor(x => x.Id).NotEmpty();
        }
    }
}

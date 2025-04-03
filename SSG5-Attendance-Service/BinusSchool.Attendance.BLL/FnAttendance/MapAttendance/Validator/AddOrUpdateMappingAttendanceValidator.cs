using BinusSchool.Data.Model.Attendance.FnAttendance.MapAttendance;
using FluentValidation;

namespace BinusSchool.Attendance.FnAttendance.Attendance.Validator
{
    public class AddOrUpdateMappingAttendanceValidator : AbstractValidator<AddOrUpdateMappingAttendanceRequest>
    {
        public AddOrUpdateMappingAttendanceValidator()
        {
            RuleFor(x => x.IdLevel).NotEmpty();

            RuleFor(x => x.AttendanceName).NotEmpty();

            RuleFor(x => x.AbsentTerms).NotNull();

            RuleFor(x => x.IsNeedValidation).NotNull();

            RuleFor(x => x.IsUseWorkHabit).NotNull();

            RuleFor(x => x.IsDueToLateness).NotNull();
            
            RuleFor(x => x.UsingCheckboxAttendance).NotNull();

            RuleFor(x => x.ShowingModalReminderAttendanceEntry).NotNull();

            RuleFor(x => x.RenderAttendance).NotNull();

        }
    }
}

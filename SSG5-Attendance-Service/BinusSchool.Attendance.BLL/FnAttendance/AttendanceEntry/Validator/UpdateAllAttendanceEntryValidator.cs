using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceEntry;
using BinusSchool.Data.Model.Attendance.FnAttendance.MapAttendance;
using FluentValidation;

namespace BinusSchool.Attendance.FnAttendance.AttendanceEntry.Validator
{
    public class UpdateAllAttendanceEntryValidator : AbstractValidator<UpdateAllAttendanceEntryRequest>
    {
        public UpdateAllAttendanceEntryValidator(GetMapAttendanceDetailResult mapAttendance)
        {
            RuleFor(x => x.Date).NotEmpty();
            
            // set ClassId & IsSession required when absent term: Session
            When(ax => mapAttendance.Term == AbsentTerm.Session, () =>
            {
                RuleFor(x => x.CurrentPosition).NotEmpty();

                RuleFor(x => x.IdSession).NotEmpty();
            });
        }
    }
}

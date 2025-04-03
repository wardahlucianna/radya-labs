using System.Linq;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceEntry;
using BinusSchool.Data.Model.Attendance.FnAttendance.MapAttendance;
using FluentValidation;

namespace BinusSchool.Attendance.FnAttendance.AttendanceEntry.Validator
{
    public class UpdateAttendanceEntryValidator : AbstractValidator<UpdateAttendanceEntryRequest>
    {
        public UpdateAttendanceEntryValidator(GetMapAttendanceDetailResult mapAttendance)
        {
            RuleFor(x => x.IdSchool).NotEmpty();

            RuleFor(x => x.Date).NotEmpty();

            RuleFor(x => x.Entries)
                .NotEmpty()
                .ForEach(entries => entries.ChildRules(entry =>
                {
                    entry.RuleFor(x => x.IdGeneratedScheduleLesson).NotEmpty();

                    entry.RuleFor(x => x.IdAttendanceMapAttendance)
                        .NotEmpty()
                        .DependentRules(() => 
                        {
                            entry.RuleFor(x => x.IdAttendanceMapAttendance)
                                .Must(x => mapAttendance.Attendances.Select(y => y.IdAttendanceMapAttendance).Contains(x))
                                .WithMessage(x => $"{nameof(x.IdAttendanceMapAttendance)} is not available for level {mapAttendance.Level.Description}.");

                            // chek when selected attendance need file attachment
                            entry.When(x => mapAttendance.Attendances
                                .FirstOrDefault(y => y.IdAttendanceMapAttendance == x.IdAttendanceMapAttendance)?
                                .NeedAttachment ?? false, () =>
                            {
                                // entry.RuleFor(x => x.File).NotEmpty();
                            });
                        });
                }));

            // set ClassId & IsSession required when absent term: Session
            When(ax => mapAttendance.Term == AbsentTerm.Session, () =>
            {
                RuleFor(x => x.CurrentPosition).NotEmpty();

                RuleFor(x => x.ClassId).NotEmpty();

                RuleFor(x => x.IdSession).NotEmpty();
            });
        }
    }
}

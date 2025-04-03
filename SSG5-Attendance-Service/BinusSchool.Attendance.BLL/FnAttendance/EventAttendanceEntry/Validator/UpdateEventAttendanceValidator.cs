using System.Linq;
using BinusSchool.Data.Model.Attendance.FnAttendance.EventAttendanceEntry;
using BinusSchool.Data.Model.Attendance.FnAttendance.MapAttendance;
using FluentValidation;

namespace BinusSchool.Attendance.FnAttendance.EventAttendanceEntry.Validator
{
    public class UpdateEventAttendanceEntryValidator : AbstractValidator<UpdateEventAttendanceEntryRequest>
    {
        public UpdateEventAttendanceEntryValidator(GetMapAttendanceDetailResult mapAttendance)
        {
            RuleFor(x => x.IdEventCheck).NotEmpty();

            RuleFor(x => x.IdLevel).NotEmpty();

            RuleFor(x => x.Entries)
                .NotEmpty()
                .ForEach(entries => entries.ChildRules(entry =>
                {
                    entry.RuleFor(x => x.IdUserEvent).NotEmpty();

                    entry.RuleFor(x => x.IdAttendanceMapAttendance)
                        .NotEmpty()
                        .DependentRules(() =>
                        {
                            entry.RuleFor(x => x.IdAttendanceMapAttendance)
                                .Must(x => mapAttendance.Attendances.Select(y => y.IdAttendanceMapAttendance).Contains(x))
                                .WithMessage(x => $"{nameof(x.IdAttendanceMapAttendance)} is not available for level {mapAttendance.Level.Description}.");

                            // chek when selected attendance need file attachment
                            //entry.When(x => mapAttendance.Attendances
                            //    .FirstOrDefault(y => y.IdAttendanceMapAttendance == x.IdAttendanceMapAttendance)?
                            //    .NeedAttachment ?? false, () =>
                            //    {
                            //        entry.RuleFor(x => x.File).NotEmpty();
                            //    });
                        });
                }));
        }
    }
}

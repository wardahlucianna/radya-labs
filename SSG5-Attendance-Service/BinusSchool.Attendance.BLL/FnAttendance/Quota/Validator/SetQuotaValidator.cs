using BinusSchool.Data.Model.Attendance.FnAttendance.Quota;
using FluentValidation;

namespace BinusSchool.Attendance.FnAttendance.Quota.Validator
{
    public class SetQuotaValidator : AbstractValidator<SetQuotaRequest>
    {
        public SetQuotaValidator()
        {
            RuleFor(x => x.IdLevel).NotNull();

            RuleFor(x => x.Quotas)
                .NotEmpty()
                .ForEach(entries => entries.ChildRules(entry =>
                {
                    entry.RuleFor(x => x.IdAttendance)
                            .NotNull();
                    entry.RuleFor(x => x.Percentage)
                            .NotNull()
                            .LessThanOrEqualTo(100);
                }));
        }
    }
}

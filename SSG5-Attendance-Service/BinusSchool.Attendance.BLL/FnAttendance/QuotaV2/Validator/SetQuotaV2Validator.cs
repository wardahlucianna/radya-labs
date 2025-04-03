using BinusSchool.Data.Model.Attendance.FnAttendance.QuotaV2;
using FluentValidation;

namespace BinusSchool.Attendance.FnAttendance.QuotaV2.Validator
{
    public class SetQuotaV2Validator : AbstractValidator<SetQuotaV2Request>
    {
        public SetQuotaV2Validator()
        {
            RuleFor(x => x.IdLevel).NotNull();
            RuleFor(x => x.IdAcademicYear).NotNull();

            RuleFor(x => x.QuotaDetails)
                .NotEmpty()
                .ForEach(entries => entries.ChildRules(entry =>
                {
                    entry.RuleFor(x => x.Percentage)
                            .NotNull()
                            .LessThanOrEqualTo(100);
                }));
        }
    }
}

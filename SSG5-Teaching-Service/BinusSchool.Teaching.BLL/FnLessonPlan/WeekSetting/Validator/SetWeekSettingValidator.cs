using BinusSchool.Data.Model.Teaching.FnLessonPlan.WeekSetting;
using FluentValidation;

namespace BinusSchool.Teaching.FnLessonPlan.WeekSetting.Validator
{
    public class SetWeekSettingValidator : AbstractValidator<SetWeekSettingRequest>
    {
        public SetWeekSettingValidator()
        {
            RuleFor(x => x.IdWeekSetting).NotEmpty();

            RuleFor(x => x.Method).NotEmpty();

            RuleFor(x => x.TotalWeek)
                .NotNull()
                .WithMessage("Total Week is required");

            RuleFor(x => x.TotalWeek)
                .NotNull()
                .GreaterThan(0)
                .WithMessage("Total Week must be greater than 0");
        }
    }
}
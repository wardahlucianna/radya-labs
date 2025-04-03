using BinusSchool.Data.Model.Teaching.FnLessonPlan.WeekSetting;
using FluentValidation;

namespace BinusSchool.Teaching.FnLessonPlan.WeekSetting.Validator
{
    public class WeekSettingDetailValidator : AbstractValidator<WeekSettingDetail>
    {
        public WeekSettingDetailValidator()
        {
            RuleFor(x => x.IdWeekSettingDetail).NotNull();

            RuleFor(x => x.WeekNumber)
                .NotNull()
                .GreaterThan(0);
            
            RuleFor(x => x.Status).NotNull();
        }
    }

    public class SaveWeekSettingDetailValidator : AbstractValidator<SaveWeekSettingDetailRequest>
    {
        public SaveWeekSettingDetailValidator()
        {
            RuleFor(x => x.IdWeekSetting).NotEmpty();

            RuleForEach(x => x.WeekSettingDetails).SetValidator(new WeekSettingDetailValidator());
        }
    }
}
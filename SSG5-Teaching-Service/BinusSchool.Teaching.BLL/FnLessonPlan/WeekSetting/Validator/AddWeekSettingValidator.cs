using BinusSchool.Data.Model.Teaching.FnLessonPlan.WeekSetting;
using FluentValidation;
using System.Linq;

namespace BinusSchool.Teaching.FnLessonPlan.WeekSetting.Validator
{
    public class AddWeekSettingValidator : AbstractValidator<AddWeekSettingRequest>
    {
        public AddWeekSettingValidator()
        {
            RuleFor(x => x.IdPeriod).NotEmpty();

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

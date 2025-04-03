using BinusSchool.Data.Model.Teaching.FnLessonPlan.LessonPlan;
using FluentValidation;

namespace BinusSchool.Teaching.FnLessonPlan.LessonPlan.Validator
{
    public class SetLessonPlanApprovalSettingValidator : AbstractValidator<SetLessonPlanApprovalSettingRequest>
    {
        public SetLessonPlanApprovalSettingValidator()
        {
            RuleFor(x => x.IdLevelApproval)
                .NotNull()
                .WithMessage("Id Level Approval is required");
            RuleFor(x => x.IsApproved)
                .NotNull()
                .WithMessage("Status Approval is required");
        }
    }
}
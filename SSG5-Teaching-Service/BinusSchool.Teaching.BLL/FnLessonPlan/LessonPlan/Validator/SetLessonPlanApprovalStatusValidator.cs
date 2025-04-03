using BinusSchool.Data.Model.Teaching.FnLessonPlan.LessonPlan;
using FluentValidation;

namespace BinusSchool.Teaching.FnLessonPlan.LessonPlan.Validator
{
    public class SetLessonPlanApprovalStatusValidator : AbstractValidator<SetLessonPlanApprovalStatusRequest>
    {
        public SetLessonPlanApprovalStatusValidator()
        {
            RuleFor(x => x.IdUser).NotEmpty();
            
            RuleFor(x => x.IdLessonPlanApproval).NotEmpty();
            
            RuleFor(x => x.IsApproved).NotNull();
        }
    }
}
using System.Collections.Generic;
using BinusSchool.Data.Model.Teaching.FnLessonPlan.LessonPlan;
using FluentValidation;

namespace BinusSchool.Teaching.FnLessonPlan.LessonPlan.Validator
{
    public class AddLessonPlanApproverSettingValidator : AbstractValidator<AddLessonPlanApproverSettingRequest>
    {
        public AddLessonPlanApproverSettingValidator()
        {
            RuleFor(x => x.IdSchool).NotEmpty();
        }
    }
}

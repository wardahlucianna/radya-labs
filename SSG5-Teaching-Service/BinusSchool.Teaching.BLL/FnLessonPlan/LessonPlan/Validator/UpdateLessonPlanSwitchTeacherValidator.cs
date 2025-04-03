using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Teaching.FnLessonPlan.LessonPlan;
using FluentValidation;

namespace BinusSchool.Teaching.FnLessonPlan.LessonPlan.Validator
{
    public class UpdateLessonPlanSwitchTeacherValidator : AbstractValidator<UpdateLessonPlanSwitchTeacherRequest>
    {
        public UpdateLessonPlanSwitchTeacherValidator()
        {
            RuleFor(x => x.IdLesson).NotEmpty();

            RuleFor(x => x.IdLessonTeacher).NotEmpty();

        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Teaching.FnLessonPlan.LessonPlan;
using FluentValidation;

namespace BinusSchool.Teaching.FnLessonPlan.LessonPlan.Validator
{
    public class DeleteLessonPlanBlobValidator : AbstractValidator<DeleteLessonPlanBlobRequest>
    {
        public DeleteLessonPlanBlobValidator()
        {
            RuleFor(x => x.FileName)
                .NotNull()
                .NotEmpty()
                .WithMessage("File Name is required");
        }
    }
}

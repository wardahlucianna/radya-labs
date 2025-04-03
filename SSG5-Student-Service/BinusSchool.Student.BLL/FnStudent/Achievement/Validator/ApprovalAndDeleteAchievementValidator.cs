using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Student.FnStudent.Achievement;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.Achievement.Validator
{
    public class ApprovalAndDeleteAchievementValidator : AbstractValidator<ApprovalAndDeleteAchievementRequest>
    {
        public ApprovalAndDeleteAchievementValidator()
        {
            RuleFor(x => x.IdAchievement).NotEmpty().WithMessage("Id Achievement cannot empty");
        }
    }
}

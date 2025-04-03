using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Student.FnStudent.Portfolio.LearningGolas;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.Portfolio.LearningGoals.Validator
{
    public class UpdateLearningAchievedValidator : AbstractValidator<UpdateLearningAchievedRequest>
    {
        public UpdateLearningAchievedValidator()
        {
            RuleFor(x => x.LearningAchieveds).NotEmpty().WithMessage("Learning achieveds cannot empty");
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Student.FnStudent.Portfolio.LearningGolas;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.Portfolio.LearningGoals.Validator
{
    public class UpdateLearningGoalsValidator : AbstractValidator<UpdateLearningGoalsRequest>
    {
        public UpdateLearningGoalsValidator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage("Id cannot empty");
            RuleFor(x => x.LearningGoals).NotEmpty().WithMessage("Learning goals cannot empty");
            RuleFor(x => x.IdLearningGoalsCategory).NotEmpty().WithMessage("Learning goals category cannot empty");
            RuleFor(x => x.IdAcademicYear).NotEmpty().WithMessage("Acedemic Year cannot empty");
            RuleFor(x => x.Semester).NotEmpty().WithMessage("Semester cannot empty");
        }
    }
}

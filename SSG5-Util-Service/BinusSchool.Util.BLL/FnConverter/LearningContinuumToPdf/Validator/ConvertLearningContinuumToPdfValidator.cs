using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Util.FnConverter.LearningContinuumToPdf;
using FluentValidation;

namespace BinusSchool.Util.FnConverter.LearningContinuumToPdf.Validator
{
    public class ConvertLearningContinuumToPdfValidator : AbstractValidator<ConvertLearningContinuumToPdfRequest>
    {
        public ConvertLearningContinuumToPdfValidator()
        {
            RuleFor(x => x.AcademicYear).NotEmpty();
            RuleFor(x => x.Grade).NotEmpty();
            RuleFor(x => x.IdSchool).NotEmpty();
            RuleFor(x => x.Semester).NotEmpty();
            RuleFor(x => x.Student).NotEmpty();
            RuleFor(x => x.SubjectContinuum).NotEmpty();
        }
    }
}

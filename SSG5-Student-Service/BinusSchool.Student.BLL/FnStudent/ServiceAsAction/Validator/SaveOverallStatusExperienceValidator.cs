using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Student.FnStudent.ServiceAsAction;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.ServiceAsAction.Validator
{
    public class SaveOverallStatusExperienceValidator : AbstractValidator<SaveOverallStatusExperienceRequest>
    {
        public SaveOverallStatusExperienceValidator()
        {
            RuleFor(x => x.IdAcademicYear).NotEmpty();
            RuleFor(x => x.IdStudent).NotEmpty();
            RuleFor(x => x.IdServiceAsActionStatus).NotEmpty();
        }
    }
}

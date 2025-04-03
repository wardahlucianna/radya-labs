using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Student.FnStudent.ServiceAsAction;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.ServiceAsAction.Validator
{
    public class DeleteExperienceServiceAsActionValidator : AbstractValidator<DeleteExperienceServiceAsActionRequest>
    {
        public DeleteExperienceServiceAsActionValidator()
        {
            RuleFor(x => x.IdServiceAsActionForm).NotNull();
        }
    }
}

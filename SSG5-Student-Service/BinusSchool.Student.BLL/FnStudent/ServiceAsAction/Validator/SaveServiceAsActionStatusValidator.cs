using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Student.FnStudent.ServiceAsAction;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.ServiceAsAction.Validator
{
    public class SaveServiceAsActionStatusValidator : AbstractValidator<SaveServiceAsActionStatusRequest>
    {
        public SaveServiceAsActionStatusValidator()
        {
            RuleFor(x => x.IdGrade).NotEmpty();
            RuleFor(x => x.IdServiceAsActionForm).NotEmpty();
            RuleFor(x => x.IdServiceAsActionStatus).NotEmpty();
        }
    }
}

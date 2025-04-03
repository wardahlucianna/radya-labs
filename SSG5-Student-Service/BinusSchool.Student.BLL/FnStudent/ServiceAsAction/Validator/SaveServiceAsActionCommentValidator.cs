using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Student.FnStudent.ServiceAsAction;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.ServiceAsAction.Validator
{
    public class SaveServiceAsActionCommentValidator : AbstractValidator<SaveServiceAsActionCommentRequest>
    {
        public SaveServiceAsActionCommentValidator()
        {
            RuleFor(x => x.IdUser).NotEmpty();
            RuleFor(x => x.IdServiceAsActionEvidence).NotEmpty();
            RuleFor(x => x.Comment).NotEmpty();
        }
    }
}

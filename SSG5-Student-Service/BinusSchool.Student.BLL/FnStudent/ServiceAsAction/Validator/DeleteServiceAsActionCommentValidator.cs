using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Student.FnStudent.ServiceAsAction;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.ServiceAsAction.Validator
{
    public class DeleteServiceAsActionCommentValidator : AbstractValidator<DeleteServiceAsActionCommentRequest>
    {
        public DeleteServiceAsActionCommentValidator()
        {
            RuleFor(x => x.IdServiceAsActionComment).NotEmpty();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Student.FnStudent.Portfolio.Reflection;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.Portfolio.Reflection.Validator
{
    public class UpdateReflectionCommentValidator : AbstractValidator<UpdateReflectionCommentRequest>
    {
        public UpdateReflectionCommentValidator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage("Id cannot empty");
            RuleFor(x => x.comment).NotEmpty().WithMessage("Comment cannot empty");
        }
    }
}

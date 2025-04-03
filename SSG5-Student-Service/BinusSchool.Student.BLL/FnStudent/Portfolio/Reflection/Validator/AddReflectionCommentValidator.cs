using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Student.FnStudent.Portfolio.Reflection;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.Portfolio.Reflection.Validator
{
    public class AddReflectionCommentValidator : AbstractValidator<AddReflectionCommentRequest>
    {
        public AddReflectionCommentValidator()
        {
            RuleFor(x => x.Comment).NotEmpty().WithMessage("Comment cannot empty");
            RuleFor(x => x.IdUser).NotEmpty().WithMessage("Id user cannot empty");
            RuleFor(x => x.IdReflection).NotEmpty().WithMessage("Id reflection cannot empty");
        }
    }
}

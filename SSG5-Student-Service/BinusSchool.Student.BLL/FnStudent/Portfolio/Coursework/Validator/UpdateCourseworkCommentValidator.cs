using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Student.FnStudent.Portfolio.Coursework;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.Portfolio.Coursework.Validator
{
    public class UpdateCourseworkCommentValidator : AbstractValidator<UpdateCourseworkCommentRequest>
    {
        public UpdateCourseworkCommentValidator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage("Id cannot empty");
            RuleFor(x => x.Comment).NotEmpty().WithMessage("Comment cannot empty");
        }
    }
}

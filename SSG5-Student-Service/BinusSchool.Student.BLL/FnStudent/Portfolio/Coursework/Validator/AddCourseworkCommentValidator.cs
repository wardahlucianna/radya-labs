using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Student.FnStudent.Portfolio.Coursework;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.Portfolio.Coursework.Validator
{
    public class AddCourseworkCommentValidator : AbstractValidator<AddCourseworkCommentRequest>
    {
        public AddCourseworkCommentValidator()
        {
            RuleFor(x => x.IdCourseworkAnecdotalStudent).NotEmpty().WithMessage("Id Coursework/Anecdotal Student cannot empty");
            RuleFor(x => x.IdUser).NotEmpty().WithMessage("Id User cannot empty");
            RuleFor(x => x.Comment).NotEmpty().WithMessage("Comment cannot empty");
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Student.FnStudent.Portfolio.Reflection;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.Portfolio.Reflection.Validator
{
    public class UpdateReflectionContentValidator : AbstractValidator<UpdateReflectionContentRequest>
    {
        public UpdateReflectionContentValidator()
        {
            RuleFor(x => x.IdSchool).NotEmpty().WithMessage("Id School cannot empty");
        }
    }
}

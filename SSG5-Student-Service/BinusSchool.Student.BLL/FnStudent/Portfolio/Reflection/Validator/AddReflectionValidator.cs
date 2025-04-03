using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Student.FnStudent.Portfolio.Reflection;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.Portfolio.Reflection.Validator
{
    public class AddReflectionValidator : AbstractValidator<AddReflectionRequest>
    {
        public AddReflectionValidator()
        {
            RuleFor(x => x.Content).NotEmpty().WithMessage("Content cannot empty");
            RuleFor(x => x.IdStudent).NotEmpty().WithMessage("Id Student cannot empty");
            RuleFor(x => x.IdAcademicYear).NotEmpty().WithMessage("Acedemic Year cannot empty");
            RuleFor(x => x.Semester).NotEmpty().WithMessage("Semester cannot empty");
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Student.FnStudent.Portfolio.Coursework;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.Portfolio.Coursework.Validator
{
    public class UpdateCourseworkValidator : AbstractValidator<UpdateCourseworkRequest>
    {
        public UpdateCourseworkValidator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage("Id empty");
            RuleFor(x => x.IdStudent).NotEmpty().WithMessage("Id Student cannot empty");
            RuleFor(x => x.Content).NotEmpty().WithMessage("Content cannot empty");
            RuleFor(x => x.IdAcademicYear).NotEmpty().WithMessage("Acedemic Year cannot empty");
            RuleFor(x => x.Semester).NotEmpty().WithMessage("Semester cannot empty");
        }
    }
}

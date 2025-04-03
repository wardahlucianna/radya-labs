using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Student.FnStudent.MeritDemeritTeacher;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.MeritDemeritTeacher.Validator
{
    public class CalculateMeritDemeritPointValidator : AbstractValidator<CalculateMeritDemeritPointRequest>
    {
        public CalculateMeritDemeritPointValidator()
        {
            RuleFor(x => x.IdSchool).NotEmpty().WithMessage("Id school year cannot empty");
        }
    }
}

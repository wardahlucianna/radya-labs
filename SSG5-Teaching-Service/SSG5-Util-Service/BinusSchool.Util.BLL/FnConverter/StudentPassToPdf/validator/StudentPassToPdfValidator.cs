using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Util.FnConverter.StudentPassToPdf;
using FluentValidation;

namespace BinusSchool.Util.FnConverter.StudentPassToPdf.validator
{
    public class StudentPassToPdfValidator : AbstractValidator<StudentPassToPdfRequest>
    {
        public StudentPassToPdfValidator()
        {
            RuleFor(x => x.IdStudent).NotNull();
            RuleFor(x => x.IdSchool).NotNull();
            RuleFor(x => x.IdStudent).NotNull();
            RuleFor(x => x.Id).NotNull();
        }
    }
}

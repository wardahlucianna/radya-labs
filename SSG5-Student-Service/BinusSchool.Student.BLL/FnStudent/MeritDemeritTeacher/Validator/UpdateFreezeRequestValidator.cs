using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.School.FnSchool.MeritDemerit;
using BinusSchool.Data.Model.Student.FnStudent.MeritDemerit;
using FluentValidation;


namespace BinusSchool.Student.FnStudent.MeritDemerit.Validator
{
    public class UpdateFreezeRequestValidator : AbstractValidator<UpdateFreezeRequest>
    {
        public UpdateFreezeRequestValidator()
        {
            RuleFor(x => x.IdHomeroomStudent).NotEmpty().WithMessage("Homeroom student cannot empty");
        }
    }
}

using System;
using System.Collections.Generic;
using BinusSchool.Common.Validators;
using BinusSchool.Data.Model.School.FnSchool.AcademicYear;
using FluentValidation;

namespace BinusSchool.School.FnSchool.AcademicYear.Validator
{
    public class AddAcademicYearValidator : AbstractValidator<AddAcademicYearRequest>
    {
        public AddAcademicYearValidator()
        {
            RuleFor(x => x.IdSchool).NotEmpty();

            RuleFor(x => x.Code)
                .NotEmpty()
                .WithName("Academic Year Short Name");
        }
    }
}

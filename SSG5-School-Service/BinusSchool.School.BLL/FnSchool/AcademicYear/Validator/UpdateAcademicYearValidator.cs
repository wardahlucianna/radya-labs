using System;
using System.Collections.Generic;
using BinusSchool.Data.Model.School.FnSchool.AcademicYear;
using FluentValidation;

namespace BinusSchool.School.FnSchool.AcademicYear.Validator
{
    public class UpdateAcademicYearValidator : AbstractValidator<UpdateAcademicYearRequest>
    {
        public UpdateAcademicYearValidator()
        {
            Include(new AddAcademicYearValidator());

            RuleFor(x => x.Id).NotEmpty();
        }
    }
}

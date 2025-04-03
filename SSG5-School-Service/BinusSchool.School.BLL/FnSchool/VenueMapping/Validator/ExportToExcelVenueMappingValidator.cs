using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.School.FnSchool.VenueMapping;
using FluentValidation;

namespace BinusSchool.School.FnSchool.VenueMapping.Validator
{
    public class ExportToExcelVenueMappingValidator : AbstractValidator<ExportToExcelVenueMappingRequest>
    {
        public ExportToExcelVenueMappingValidator()
        {
            RuleFor(x => x.IdAcademicYear)
                .NotEmpty()
                .WithMessage("Id Academic Year is required");
        }
    }
}

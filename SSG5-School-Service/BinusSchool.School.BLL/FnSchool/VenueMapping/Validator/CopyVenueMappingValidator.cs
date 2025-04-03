using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.School.FnSchool.VenueMapping;
using FluentValidation;

namespace BinusSchool.School.FnSchool.VenueMapping.Validator
{
    public class CopyVenueMappingValidator : AbstractValidator<CopyVenueMappingRequest>
    {
        public CopyVenueMappingValidator()
        {
            RuleFor(x => x.IdAcademicYearDestination).NotEmpty();
            RuleFor(x => x.IdAcademicYearSource).NotEmpty();
        }
    }
}

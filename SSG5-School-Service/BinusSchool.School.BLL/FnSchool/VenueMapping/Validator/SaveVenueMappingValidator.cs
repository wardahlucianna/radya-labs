using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BinusSchool.Data.Model.School.FnSchool.VenueMapping;
using FluentValidation;

namespace BinusSchool.School.FnSchool.VenueMapping.Validator
{
    public class SaveVenueMappingValidator: AbstractValidator<SaveVenueMappingRequest>
    {
        public SaveVenueMappingValidator()
        {
            RuleFor(x => x.IdAcademicYear).NotEmpty();
            RuleFor(x => x.VenueMappingDatas).NotEmpty();
        }
    }
}

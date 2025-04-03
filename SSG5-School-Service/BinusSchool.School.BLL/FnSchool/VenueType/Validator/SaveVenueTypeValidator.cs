using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.School.FnSchool.VenueType;
using FluentValidation;

namespace BinusSchool.School.FnSchool.VenueType.Validator
{
    public class SaveVenueTypeValidator : AbstractValidator<SaveVenueTypeRequest>
    {
        public SaveVenueTypeValidator()
        {
            RuleFor(x => x.IdSchool).NotEmpty().WithMessage("Id School is required");
            RuleFor(x => x.VenueTypeName).NotEmpty().WithMessage("Venue Type Name is required");
            RuleFor(x => x.Description).NotEmpty().WithMessage("Description is required");
        }
    }
}

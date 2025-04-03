using System;
using System.Collections.Generic;
using BinusSchool.Data.Model.School.FnSchool.Venue;
using FluentValidation;

namespace BinusSchool.School.FnSchool.Venue.Validator
{
    public class UpdateVenueValidator : AbstractValidator<UpdateVenueRequest>
    {
        public UpdateVenueValidator()
        {
            Include(new AddVenueValidator());

            RuleFor(x => x.Id).NotEmpty();
        }
    }
}

using System;
using System.Collections.Generic;
using BinusSchool.Common.Validators;
using BinusSchool.Data.Model.School.FnSchool.Venue;
using FluentValidation;

namespace BinusSchool.School.FnSchool.Venue.Validator
{
    public class AddVenueValidator : AbstractValidator<AddVenueRequest>
    {
        public AddVenueValidator()
        {
            RuleFor(x => x.IdBuilding).NotEmpty();

            RuleFor(x => x.Code)
                .NotEmpty()
                .WithName("Venue Name");

            RuleFor(x => x.Capacity)
                .GreaterThanOrEqualTo(0)
                .LessThan(int.MaxValue);
        }
    }
}

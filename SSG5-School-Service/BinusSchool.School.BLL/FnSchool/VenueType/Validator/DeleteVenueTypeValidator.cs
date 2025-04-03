using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.School.FnSchool.VenueType;
using FluentValidation;

namespace BinusSchool.School.FnSchool.VenueType.Validator
{
    public class DeleteVenueTypeValidator : AbstractValidator<DeleteVenueTypeRequest>
    {
        public DeleteVenueTypeValidator()
        {
            RuleFor(x => x.IdVenueType).NotEmpty();
        }
    }
}

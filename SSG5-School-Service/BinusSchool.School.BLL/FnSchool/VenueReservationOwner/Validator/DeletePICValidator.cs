using System;
using System.Collections.Generic;
using BinusSchool.Common.Validators;
using BinusSchool.Data.Model.School.FnSchool.VenueReservationOwner;
using FluentValidation;

namespace BinusSchool.School.FnSchool.VenueReservationOwner.Validator
{
    internal class DeletePICValidator : AbstractValidator<DeletePICOwnerRequest>
    {
        public DeletePICValidator()
        {
            RuleFor(x => x.IdReservationOwner).NotEmpty();
        }
    } 
}

using System;
using System.Collections.Generic;
using BinusSchool.Common.Validators;
using BinusSchool.Data.Model.School.FnSchool.VenueReservationOwner;
using FluentValidation;

namespace BinusSchool.School.FnSchool.VenueReservationOwner.Validator
{
    internal class SavePICValidator : AbstractValidator<SavePicOwnerRequest>
    {
        public SavePICValidator() 
        {
            RuleFor(x => x.IdSchool).NotEmpty();
            RuleFor(x => x.OwnerName).NotEmpty();
            RuleFor(x => x.PICEmail).NotEmpty();
        }
    }
}

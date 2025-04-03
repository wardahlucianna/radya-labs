using System;
using System.Collections.Generic;
using BinusSchool.Common.Validators;
using BinusSchool.Data.Model.School.FnSchool.VenueEquipment;
using FluentValidation;

namespace BinusSchool.School.FnSchool.VenueEquipment.Validator
{
    internal class DeleteVanueEquipmentValidator : AbstractValidator<DeleteVenueEquipmentRequest>
    {
        public DeleteVanueEquipmentValidator()
        {
            RuleFor(x => x.IdVenue).NotEmpty();
        }
    }
}

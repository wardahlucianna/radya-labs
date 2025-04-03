using System;
using System.Collections.Generic;
using BinusSchool.Common.Validators;
using BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservation.BookingEquipmentOnly;
using BinusSchool.Data.Model.School.FnSchool.VenueEquipment;
using FluentValidation;

namespace BinusSchool.School.FnSchool.VenueEquipment.Validator
{
    public class SaveVanueEquipValidator : AbstractValidator<SaveVenueEquipRequest>
    {
        public SaveVanueEquipValidator() 
        {
            RuleFor(x => x.Status).NotNull();
            RuleFor(x => x.EquipmentList).NotEmpty();
            When(x => x.EquipmentList != null, () =>
            {
                RuleForEach(x => x.EquipmentList)
                .SetValidator(new SaveVanueEquipmentValidators());
            });
        }
    }

    public class SaveVanueEquipmentValidators : AbstractValidator<SaveVenueEquipmentRequest>
    {
        public SaveVanueEquipmentValidators()
        {
            RuleFor(x => x.IdVenue).NotEmpty();
            RuleFor(x => x.IdEquipment).NotEmpty();
            RuleFor(x => x.EquipmentQty).NotEmpty();
        }
    }
}

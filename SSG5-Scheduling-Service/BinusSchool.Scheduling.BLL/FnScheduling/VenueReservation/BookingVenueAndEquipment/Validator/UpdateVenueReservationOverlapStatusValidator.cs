using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservation.BookingVenueAndEquipment;
using FluentValidation;

namespace BinusSchool.Scheduling.FnSchedule.VenueReservation.BookingVenueAndEquipment.Validator
{
    public class UpdateVenueReservationOverlapStatusValidator : AbstractValidator<UpdateVenueReservationOverlapStatusRequest>
    {
        public UpdateVenueReservationOverlapStatusValidator()
        {
            RuleFor(a => a.IdUser)
                .NotEmpty();
        }
    }
}

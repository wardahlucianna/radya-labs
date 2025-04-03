using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservation.BookingVenueAndEquipment;
using FluentValidation;

namespace BinusSchool.Scheduling.FnSchedule.VenueReservation.BookingVenueAndEquipment.Validator
{
    public class DeleteVenueReservationBookingValidator : AbstractValidator<DeleteVenueReservationBookingRequest>
    {
        public DeleteVenueReservationBookingValidator()
        {
            RuleFor(x => x.IdVenueReservation)
                .NotEmpty();
        }
    }
}

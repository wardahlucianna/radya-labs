using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservation.BookingVenueAndEquipment;
using FluentValidation;

namespace BinusSchool.Scheduling.FnSchedule.VenueReservation.BookingVenueAndEquipment.Validator
{
    public class SaveVenueReservationBookingValidator : AbstractValidator<List<SaveVenueReservationBookingRequest>>
    {
        public SaveVenueReservationBookingValidator()
        {
            RuleFor(req => req)
                .NotEmpty()
                .ForEach(data => data.ChildRules(data =>
                {

                }));
        }
    }
}

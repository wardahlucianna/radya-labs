using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservation.BookingVenueAndEquipment;
using FluentValidation;

namespace BinusSchool.Scheduling.FnSchedule.VenueReservation.BookingVenueAndEquipment.Validator
{
    public class GetVenueReservationOverlapCheckValidator : AbstractValidator<GetVenueReservationOverlapCheckRequest>
    {
        public GetVenueReservationOverlapCheckValidator()
        {
        }
    }
}

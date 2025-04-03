using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservation.BookingVenueAndEquipment;
using FluentValidation;

namespace BinusSchool.Scheduling.FnSchedule.VenueReservation.BookingVenueAndEquipment.Validator
{
    public class UpdateVenueReservationPrepAndCleanTimeValidator : AbstractValidator<UpdateVenueReservationPrepAndCleanTimeRequest>
    {
        public UpdateVenueReservationPrepAndCleanTimeValidator()
        {
            RuleFor(a => a.IdUser)
                .NotEmpty();

            RuleFor(a => a.IdVenueReservation)
                .NotEmpty();

            //RuleFor(a => a.PreparationTime)
            //    .NotEmpty();

            //RuleFor(a => a.CleanUpTime)
            //    .NotEmpty();
        }
    }
}

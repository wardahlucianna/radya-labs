using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservationSettings.MasterSpecialRoleVenueReservation;
using FluentValidation;

namespace BinusSchool.Scheduling.FnSchedule.VenueReservationSettings.Validator
{
    public class SaveSpecialRoleVenueValidator : AbstractValidator<SaveSpecialRoleVenueRequest>
    {
        public SaveSpecialRoleVenueValidator()
        {
            RuleFor(x => x.IdRole).NotEmpty();
            RuleFor(x => x.SpecialDurationBookingTotalDay).NotEmpty().InclusiveBetween(0, 365).WithMessage("Maximal input Special Duration Booking (Days) is 365 days");
        }
    }
}

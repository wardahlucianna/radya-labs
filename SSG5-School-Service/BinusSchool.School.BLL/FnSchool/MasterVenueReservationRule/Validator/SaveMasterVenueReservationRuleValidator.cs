using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.School.FnSchool.MasterVenueReservationRule;
using FluentValidation;

namespace BinusSchool.School.FnSchool.MasterVenueReservationRule.Validator
{
    public class SaveMasterVenueReservationRuleValidator : AbstractValidator<SaveMasterVenueReservationRuleRequest>
    {
        public SaveMasterVenueReservationRuleValidator()
        {
            RuleFor(x => x.MaxDayBookingVenue).InclusiveBetween(0, 365);
            RuleFor(x => x.MaxDayDurationBookingVenue).InclusiveBetween(0, 365);
            RuleFor(x => x.StartTimeOperational).NotEmpty();
            RuleFor(x => x.EndTimeOperational).NotEmpty();
        }
    }
}

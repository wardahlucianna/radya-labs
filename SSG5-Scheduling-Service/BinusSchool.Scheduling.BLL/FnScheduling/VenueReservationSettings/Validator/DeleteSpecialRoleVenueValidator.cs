using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservationSettings.MasterSpecialRoleVenueReservation;
using FluentValidation;

namespace BinusSchool.Scheduling.FnSchedule.VenueReservationSettings.Validator
{
    public class DeleteSpecialRoleVenueValidator : AbstractValidator<DeleteSpecialRoleVenueRequest>
    {
        public DeleteSpecialRoleVenueValidator()
        {
            RuleFor(x => x.IdSpecialRoleVenue).NotEmpty();
        }
    }
}

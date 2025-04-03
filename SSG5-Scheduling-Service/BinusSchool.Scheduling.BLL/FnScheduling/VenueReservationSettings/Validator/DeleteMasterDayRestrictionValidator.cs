using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservationSettings.MasterDayRestriction;
using FluentValidation;

namespace BinusSchool.Scheduling.FnSchedule.VenueReservationSettings.Validator
{
    public class DeleteMasterDayRestrictionValidator : AbstractValidator<DeleteMasterDayRestrictionRequest>
    {
        public DeleteMasterDayRestrictionValidator()
        {
            RuleFor(x => x.IdGroupRestriction).NotEmpty();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservationSettings.MasterDayRestriction;
using FluentValidation;

namespace BinusSchool.Scheduling.FnSchedule.VenueReservationSettings.Validator
{
    public class SaveMasterDayRestrictionValidator : AbstractValidator<SaveMasterDayRestrictionRequest>
    {
        public SaveMasterDayRestrictionValidator()
        {
            RuleFor(x => x.IdSchool).NotEmpty();
            RuleFor(x => x.StartRestrictionDate).NotEmpty();
            RuleFor(x => x.EndRestrictionDate).NotEmpty();
        }
    }
}

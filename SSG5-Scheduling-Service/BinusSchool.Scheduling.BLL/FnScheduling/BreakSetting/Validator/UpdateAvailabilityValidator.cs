using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnSchedule.BreakSetting;
using FluentValidation;

namespace BinusSchool.Scheduling.FnSchedule.BreakSetting.Validator
{
    public class UpdateAvailabilityValidator : AbstractValidator<UpdateAvailabilityRequest>
    {
        public UpdateAvailabilityValidator()
        {
            RuleFor(x => x.IdInvitationBookingSettingSchedule).NotEmpty();
        }
    }
}

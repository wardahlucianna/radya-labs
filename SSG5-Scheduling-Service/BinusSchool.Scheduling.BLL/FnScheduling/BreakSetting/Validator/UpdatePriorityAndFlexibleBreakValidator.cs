using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnSchedule.BreakSetting;
using FluentValidation;

namespace BinusSchool.Scheduling.FnSchedule.BreakSetting.Validator
{
    public class UpdatePriorityAndFlexibleBreakValidator : AbstractValidator<UpdatePriorityAndFlexibleBreakRequest>
    {
        public UpdatePriorityAndFlexibleBreakValidator()
        {
            RuleFor(x => x.IdInvitationBookingSettingSchedule).NotEmpty();
        }
    }

}

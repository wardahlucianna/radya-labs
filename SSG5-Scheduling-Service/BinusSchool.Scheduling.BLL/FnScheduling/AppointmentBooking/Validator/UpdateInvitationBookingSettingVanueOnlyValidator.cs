using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnSchedule.AppointmentBooking;
using FluentValidation;

namespace BinusSchool.Scheduling.FnSchedule.AppointmentBooking.Validator
{
    public class UpdateInvitationBookingSettingVanueOnlyValidator : AbstractValidator<UpdateInvitationBookingSettingVanueOnlyRequest>
    {
        public UpdateInvitationBookingSettingVanueOnlyValidator()
        {
            RuleFor(x => x.IdInvitationBookingSetting).NotEmpty();
        }
    }
}

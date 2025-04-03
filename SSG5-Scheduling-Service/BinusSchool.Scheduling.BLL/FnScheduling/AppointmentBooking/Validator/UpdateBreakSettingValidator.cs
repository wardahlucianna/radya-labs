using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnSchedule.AppointmentBooking;
using FluentValidation;

namespace BinusSchool.Scheduling.FnSchedule.AppointmentBooking.Validator
{
    public class UpdateBreakSettingValidator : AbstractValidator<UpdateBreakSettingRequest>
    {
        public UpdateBreakSettingValidator()
        {
            RuleFor(x => x.IdInvitationBookingSettingBreak).NotEmpty();
            RuleFor(x => x.BreakName).NotEmpty();
            RuleFor(x => x.StartTime).NotEmpty();
            RuleFor(x => x.EndTime).NotEmpty();
            // RuleFor(x => x.BreakType).NotEmpty();
            RuleFor(x => x.AppointmentDate).NotEmpty();
        }
    }
}

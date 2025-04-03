using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnSchedule.AppointmentBooking;
using FluentValidation;

namespace BinusSchool.Scheduling.FnSchedule.AppointmentBooking.Validator
{
    public class AddBreakSettingValidator : AbstractValidator<AddBreakSettingRequest>
    {
        public AddBreakSettingValidator()
        {
            RuleFor(x => x.IdInvitationBookingSetting).NotEmpty();
            RuleFor(x => x.BreakName).NotEmpty();
            RuleFor(x => x.StartTime).NotEmpty();
            RuleFor(x => x.EndTime).NotEmpty();
            // RuleFor(x => x.BreakType).NotEmpty();
            RuleFor(x => x.AppointmentDate).NotEmpty();
        }
    }
}

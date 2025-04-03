using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnSchedule.AppointmentBooking;
using FluentValidation;

namespace BinusSchool.Scheduling.FnSchedule.AppointmentBooking.Validator
{
    public class CreateInvitationBookingSettingValidator : AbstractValidator<CreateInvitationBookingSettingRequest>
    {
        public CreateInvitationBookingSettingValidator()
        {
            // RuleFor(x => x.IdAcademicYear).NotEmpty();
            // RuleFor(x => x.InvitationName).NotEmpty();
            // RuleFor(x => x.InvitationStartDate).NotEmpty();
            // RuleFor(x => x.InvitationEndDate).NotEmpty();
            // RuleFor(x => x.InvitationType).NotEmpty();
            // RuleFor(x => x.ParentBookingStartDate).NotEmpty();
            // RuleFor(x => x.ParentBookingEndDate).NotEmpty();
            // RuleFor(x => x.SchedulingSiblingSameTime).NotEmpty();
            RuleFor(x => x.StepWizard).NotEmpty();
        }
    }
}

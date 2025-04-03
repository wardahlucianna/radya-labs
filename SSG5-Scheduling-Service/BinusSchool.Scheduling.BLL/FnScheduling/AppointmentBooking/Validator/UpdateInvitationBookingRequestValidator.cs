using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnSchedule.AppointmentBooking;
using FluentValidation;

namespace BinusSchool.Scheduling.FnSchedule.AppointmentBooking.Validator
{
    public class UpdateInvitationBookingRequestValidator : AbstractValidator<UpdateInvitationBookingRequest>
    {
        public UpdateInvitationBookingRequestValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
        }

    }
}

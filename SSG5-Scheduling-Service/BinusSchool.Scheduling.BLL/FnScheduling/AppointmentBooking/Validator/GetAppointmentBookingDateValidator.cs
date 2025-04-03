using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnSchedule.AppointmentBooking;
using FluentValidation;

namespace BinusSchool.Scheduling.FnSchedule.AppointmentBooking.Validator
{
    public class GetAppointmentBookingDateValidator : AbstractValidator<GetAppointmentBookingDateRequest>
    {
        public GetAppointmentBookingDateValidator()
        {
            RuleFor(x => x.IdInvitationBookingSetting).NotEmpty();
            RuleFor(x => x.IdUser).NotEmpty();
            RuleFor(x => x.Role).NotEmpty();
        }
    }
}

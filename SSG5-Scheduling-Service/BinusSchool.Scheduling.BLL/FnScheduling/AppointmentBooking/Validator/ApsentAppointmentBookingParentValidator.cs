using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnSchedule.AppointmentBooking;
using FluentValidation;

namespace BinusSchool.Scheduling.FnSchedule.AppointmentBooking.Validator
{
    public class ApsentAppointmentBookingParentValidator : AbstractValidator<ApsentAppointmentBookingParentRequest>
    {
        public ApsentAppointmentBookingParentValidator()
        {
            RuleFor(x => x.Absents.Count>0).NotEmpty();
        }
    }
}

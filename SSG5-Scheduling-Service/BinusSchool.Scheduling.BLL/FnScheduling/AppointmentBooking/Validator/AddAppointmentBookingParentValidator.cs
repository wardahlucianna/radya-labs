using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnSchedule.AppointmentBooking;
using FluentValidation;

namespace BinusSchool.Scheduling.FnSchedule.AppointmentBooking.Validator
{
    public class AddAppointmentBookingParentValidator : AbstractValidator<AddAppointmentBookingParentRequest>
    {
        public AddAppointmentBookingParentValidator()
        {
            RuleFor(x => x.IdInvitationBookingSetting).NotEmpty();
            RuleFor(x => x.IdVenue).NotEmpty();
            RuleFor(x => x.IdUserTeacher).NotEmpty();
            RuleFor(x => x.StartDateTimeInvitation).NotEmpty();
            RuleFor(x => x.EndDateTimeInvitation).NotEmpty();
            RuleFor(x => x.IdHomeroomStudents.Count>0).NotEmpty();
        }
    }
}

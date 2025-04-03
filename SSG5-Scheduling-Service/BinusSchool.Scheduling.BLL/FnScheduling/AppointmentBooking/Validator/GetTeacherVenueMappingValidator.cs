using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnSchedule.AppointmentBooking;
using FluentValidation;

namespace BinusSchool.Scheduling.FnSchedule.AppointmentBooking.Validator
{
    public class GetTeacherVenueMappingValidator : AbstractValidator<GetTeacherVenueMappingRequest>
    {
        public GetTeacherVenueMappingValidator()
        {
            RuleFor(x => x.IdInvitationBookingSetting).NotEmpty();
            RuleFor(x => x.InvitationDate).NotEmpty();
            RuleFor(x => x.IdHomeroomStudents.Count > 0).NotEmpty();
        }
    }
}

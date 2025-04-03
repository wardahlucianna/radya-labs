using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnSchedule.AppointmentBooking;
using FluentValidation;

namespace BinusSchool.Scheduling.FnSchedule.AppointmentBooking.Validator
{
    public class GetUserForUserVenueValidator : AbstractValidator<GetUserForUserVenueRequest>
    {
        public GetUserForUserVenueValidator()
        {
            RuleFor(x => x.IdInvitationBookingSetting).NotEmpty();
            RuleFor(x => x.IdRole).NotEmpty();
            RuleFor(x => x.CodePosition).NotEmpty();
        }
    }
}

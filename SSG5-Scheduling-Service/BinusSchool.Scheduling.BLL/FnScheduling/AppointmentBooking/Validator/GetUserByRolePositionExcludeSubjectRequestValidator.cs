using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnSchedule.AppointmentBooking;
using FluentValidation;

namespace BinusSchool.Scheduling.FnSchedule.AppointmentBooking.Validator
{
    public class GetUserByRolePositionExcludeSubjectRequestValidator : AbstractValidator<GetUserByRolePositionExcludeSubjectRequest>
    {
        public GetUserByRolePositionExcludeSubjectRequestValidator()
        {
            RuleFor(x => x.IdAcademicYear).NotEmpty();
            RuleFor(x => x.IdRole).NotEmpty();
        }
    }
}

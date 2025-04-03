using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolEvent;
using FluentValidation;

namespace BinusSchool.Scheduling.FnSchedule.Award.Validator
{
    public class UpdateSchoolEventInvolvementValidator : AbstractValidator<UpdateSchoolEventInvolvementRequest>
    {
        public UpdateSchoolEventInvolvementValidator()
        {
            RuleFor(x => x.IdEvent).NotEmpty().WithMessage("Event id cannot empty");
            RuleFor(x => x.IdUser).NotEmpty().WithMessage("User id cannot empty");
            RuleFor(x => x.EventName).NotEmpty().WithMessage("Event name cannot empty");
            RuleFor(x => x.IdAcadyear).NotEmpty().WithMessage("Acadmic Year cannot empty");
            RuleFor(x => x.IdEventType).NotEmpty().WithMessage("Event Type cannot empty");
            RuleFor(x => x.Dates).NotEmpty().WithMessage("Date cannot empty");
        }
    }
}

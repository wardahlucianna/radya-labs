using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolEvent;
using FluentValidation;

namespace BinusSchool.Scheduling.FnSchedule.Award.Validator
{
    public class CreateSchoolEventInvolvementValidator : AbstractValidator<CreateSchoolEventInvolvementRequest>
    {
        public CreateSchoolEventInvolvementValidator()
        {
            RuleFor(x => x.EventName).NotEmpty().WithMessage("Event name cannot empty");
            RuleFor(x => x.IdAcadyear).NotEmpty().WithMessage("Acadmic Year cannot empty");
            RuleFor(x => x.IdEventType).NotEmpty().WithMessage("Event Type cannot empty");
            RuleFor(x => x.Dates).NotEmpty().WithMessage("Date cannot empty");
            //RuleFor(x => x.Activity).NotEmpty().WithMessage("Activity cannot empty");
        }
    }
}

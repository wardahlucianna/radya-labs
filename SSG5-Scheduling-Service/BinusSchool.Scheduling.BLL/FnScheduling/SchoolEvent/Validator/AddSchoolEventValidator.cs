using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolEvent;
using FluentValidation;

namespace BinusSchool.Scheduling.FnSchedule.Award.Validator
{
    public class AddSchoolEventValidator : AbstractValidator<AddSchoolEventRequest>
    {
        public AddSchoolEventValidator()
        {
            RuleFor(x => x.EventName).NotEmpty().WithMessage("Event name cannot empty");
            RuleFor(x => x.IdAcadyear).NotEmpty().WithMessage("Acadmic Year cannot empty");
            RuleFor(x => x.IdEventType).NotEmpty().WithMessage("Event Type cannot empty");
            // RuleFor(x => x.EventObjective).NotEmpty().WithMessage("Event Objectives cannot empty");
            // RuleFor(x => x.EventPlace).NotEmpty().WithMessage("Event Place cannot empty"); ;
            // RuleFor(x => x.EventLevel).NotEmpty().WithMessage("School Event Level cannot empty");
            RuleFor(x => x.Dates).NotEmpty().WithMessage("Date cannot empty");
            // RuleFor(x => x.IdUserCoordinator).NotEmpty().WithMessage("Coordinator cannot empty"); ;
            RuleFor(x => x.IdUserEventApproval1).NotEmpty().WithMessage("Event Approver cannot empty");
            RuleFor(x => x.IntendedFor).NotEmpty().WithMessage("Intended For Type cannot empty");
            // RuleFor(x => x.Activity).NotEmpty().WithMessage("Activity cannot empty");
        }
    }
}

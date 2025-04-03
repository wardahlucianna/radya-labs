using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolEvent;
using FluentValidation;

namespace BinusSchool.Scheduling.FnSchedule.SchoolEvent.Validator
{
    public class AddHistoryEventValidator : AbstractValidator<AddHistoryEventRequest>
    {
        public AddHistoryEventValidator()
        {
            RuleFor(x => x.IdEvent).NotEmpty().WithMessage("Id Event cannot empty");
            RuleFor(x => x.IdUser).NotEmpty().WithMessage("Id User cannot empty");
            RuleFor(x => x.ActionType).NotEmpty().WithMessage("Action Type cannot empty");
        }
    }
}

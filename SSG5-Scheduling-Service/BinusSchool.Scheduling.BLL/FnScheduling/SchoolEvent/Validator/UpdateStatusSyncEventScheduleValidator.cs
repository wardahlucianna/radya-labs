using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolEvent;
using FluentValidation;

namespace BinusSchool.Scheduling.FnSchedule.SchoolEvent.Validator
{
    public class UpdateStatusSyncEventScheduleValidator : AbstractValidator<UpdateStatusSyncEventScheduleRequest>
    {
        public UpdateStatusSyncEventScheduleValidator()
        {
            RuleFor(x => x.IdEventSchedule).NotEmpty().WithMessage("Id Event Schedule cannot empty");
        }
    }
}

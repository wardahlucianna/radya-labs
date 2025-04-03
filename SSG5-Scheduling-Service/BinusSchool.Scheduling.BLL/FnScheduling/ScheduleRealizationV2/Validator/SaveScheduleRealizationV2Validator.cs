using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnSchedule.ScheduleRealization;
using BinusSchool.Data.Model.Scheduling.FnSchedule.ScheduleRealizationV2;
using FluentValidation;

namespace BinusSchool.Scheduling.FnSchedule.ScheduleRealizationV2.Validator
{
    public class SaveScheduleRealizationV2Validator : AbstractValidator<SaveScheduleRealizationV2Request>
    {
        public SaveScheduleRealizationV2Validator()
        {
            RuleFor(x => x.DataScheduleRealizations).NotNull().WithMessage("Ids cannot empty");
        }
    }
}

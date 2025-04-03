using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnSchedule.ScheduleRealization;
using FluentValidation;

namespace BinusSchool.Scheduling.FnSchedule.ScheduleRealization.Validator
{
    public class SaveSettingEmailScheduleRealizationValidator : AbstractValidator<SaveSettingEmailScheduleRealizationRequest>
    {
        public SaveSettingEmailScheduleRealizationValidator()
        {
            // RuleFor(x => x.DataSettingEmailScheduleRealizations).NotNull().WithMessage("Data Setting Email Schedule Realization cannot empty");
        }
    }
}

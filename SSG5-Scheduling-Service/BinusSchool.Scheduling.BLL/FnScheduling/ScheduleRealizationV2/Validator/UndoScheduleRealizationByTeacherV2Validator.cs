using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnSchedule.ScheduleRealization;
using BinusSchool.Data.Model.Scheduling.FnSchedule.ScheduleRealizationV2;
using FluentValidation;

namespace BinusSchool.Scheduling.FnSchedule.ScheduleRealizationV2.Validator
{
    public class UndoScheduleRealizationByTeacherV2Validator : AbstractValidator<UndoScheduleRealizationByTeacherV2Request>
    {
        public UndoScheduleRealizationByTeacherV2Validator()
        {
            RuleFor(x => x.DataScheduleRealizations).NotNull().WithMessage("Ids cannot empty");
        }
    }
}

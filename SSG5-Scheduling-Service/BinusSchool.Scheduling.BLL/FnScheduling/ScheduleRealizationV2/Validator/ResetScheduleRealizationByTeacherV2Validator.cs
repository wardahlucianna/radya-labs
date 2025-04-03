using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnSchedule.ScheduleRealization;
using BinusSchool.Data.Model.Scheduling.FnSchedule.ScheduleRealizationV2;
using FluentValidation;

namespace BinusSchool.Scheduling.FnSchedule.ScheduleRealizationV2.Validator
{
    public class ResetScheduleRealizationByTeacherV2Validator : AbstractValidator<ResetScheduleRealizationByTeacherRequest>
    {
        public ResetScheduleRealizationByTeacherV2Validator()
        {
            RuleFor(x => x.DataScheduleRealizations).NotNull().WithMessage("Ids cannot empty");
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnSchedule.ScheduleRealization;
using BinusSchool.Data.Model.Scheduling.FnSchedule.ScheduleRealizationV2;
using FluentValidation;

namespace BinusSchool.Scheduling.FnSchedule.ScheduleRealizationV2.Validator
{
    public class SendEmailForCancelClassV2Validator : AbstractValidator<SendEmailForCancelClassV2Request>
    {
        public SendEmailForCancelClassV2Validator()
        {
        }
    }
}

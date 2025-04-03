using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnSchedule.GenerateSchedule;
using FluentValidation;

namespace BinusSchool.Scheduling.FnSchedule.GenerateSchedule.Validator
{
    public class FinishGeneratedScheduleProcessValidator : AbstractValidator<FinishGeneratedScheduleProcessRequest>
    {
        public FinishGeneratedScheduleProcessValidator()
        {
            RuleFor(x => x.IdProcess).NotEmpty();
        }
    }
}

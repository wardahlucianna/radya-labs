using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnSchedule.ScheduleRealization;
using FluentValidation;

namespace BinusSchool.Scheduling.FnSchedule.ScheduleRealization.Validator
{
    public class GetDownloadScheduleRealizationValidator : AbstractValidator<GetDownloadScheduleRealizationRequest>
    {
        public GetDownloadScheduleRealizationValidator()
        {
            RuleFor(x => x.IsByDate).NotNull().WithMessage("Flag cannot empty");
            RuleFor(x => x.Ids).NotNull().WithMessage("Ids cannot empty");
            RuleFor(x => x.StartDate).NotNull().WithMessage("Start Date cannot empty");
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolEvent;
using FluentValidation;

namespace BinusSchool.Scheduling.FnSchedule.Award.Validator
{
    public class SetDefaultEventApproverSettingValidator : AbstractValidator<SetDefaultEventApproverSettingRequest>
    {
        public SetDefaultEventApproverSettingValidator()
        {
            // RuleFor(x => x.IdApprover1).NotNull().WithMessage("Approver cannot null");
        }
    }
}

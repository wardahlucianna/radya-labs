using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolEvent;
using FluentValidation;

namespace BinusSchool.Scheduling.FnSchedule.Award.Validator
{
    public class SetSchoolEventApprovalStatusValidator : AbstractValidator<SetSchoolEventApprovalStatusRequest>
    {
        public SetSchoolEventApprovalStatusValidator()
        {
            RuleFor(x => x.IdEvent).NotNull().WithMessage("Id Event cannot null");
            RuleFor(x => x.IdUser).NotNull().WithMessage("Id User cannot null");
            RuleFor(x => x.IsApproved).NotNull().WithMessage("Status Approval cannot null");
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnSchedule.PersonalInvitation;
using FluentValidation;

namespace BinusSchool.Scheduling.FnSchedule.PersonalInvitation.Validator
{
    public class UpdatePersonalInvitationApprovalValidator : AbstractValidator<UpdatePersonalInvitationApprovalRequest>
    {
        public UpdatePersonalInvitationApprovalValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
        }
    }
}

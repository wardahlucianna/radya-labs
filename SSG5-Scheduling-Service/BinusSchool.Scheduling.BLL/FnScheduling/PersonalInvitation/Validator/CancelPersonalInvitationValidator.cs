using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnSchedule.PersonalInvitation;
using FluentValidation;

namespace BinusSchool.Scheduling.FnSchedule.PersonalInvitation.Validator
{
    internal class CancelPersonalInvitationValidator : AbstractValidator<CancelPersonalInvitationRequest>
    {
        public CancelPersonalInvitationValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Reason).NotEmpty();
        }

}
}

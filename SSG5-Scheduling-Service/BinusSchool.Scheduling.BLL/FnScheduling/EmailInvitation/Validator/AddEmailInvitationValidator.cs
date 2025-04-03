using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnSchedule.EmailInvitation;
using FluentValidation;

namespace BinusSchool.Scheduling.FnSchedule.EmailInvitation.Validator
{
    public class AddEmailInvitationValidator : AbstractValidator<AddEmailInvitationRequest>
    {
        public AddEmailInvitationValidator()
        {
            RuleFor(x => x.IdInvitationBookingSetting).NotEmpty();
            RuleFor(x => x.IdHomeroomStudent).NotEmpty();
        }
}
}

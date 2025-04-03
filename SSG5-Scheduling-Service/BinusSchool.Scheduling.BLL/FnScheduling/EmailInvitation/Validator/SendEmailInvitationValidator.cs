using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnSchedule.EmailInvitation;
using FluentValidation;


namespace BinusSchool.Scheduling.FnSchedule.EmailInvitation.Validator
{
    public class SendEmailInvitationValidator : AbstractValidator<SendEmailInvitationRequest>
    {
        public SendEmailInvitationValidator()
        {
        }
    }
}

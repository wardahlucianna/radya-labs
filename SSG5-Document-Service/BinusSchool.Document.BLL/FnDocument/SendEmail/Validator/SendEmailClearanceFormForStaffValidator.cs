using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Document.FnDocument.SendEmail;
using FluentValidation;

namespace BinusSchool.Document.FnDocument.SendEmail.Validator
{
    public class SendEmailClearanceFormForStaffValidator : AbstractValidator<SendEmailClearanceFormForStaffRequest>
    {
        public SendEmailClearanceFormForStaffValidator()
        {
            RuleFor(x => x.IdSchool)
                .NotEmpty();

            RuleFor(x => x.IdSurveyPeriod)
                .NotEmpty();

            //RuleFor(x => x.IdClearanceWeekPeriod)
            //    .NotEmpty();

            RuleFor(x => x.StudentSurveyData)
                .NotEmpty();

            RuleFor(x => x.BLPFinalStatus)
                .NotNull();

            RuleFor(x => x.AuditAction)
                .NotNull();
        }
    }
}

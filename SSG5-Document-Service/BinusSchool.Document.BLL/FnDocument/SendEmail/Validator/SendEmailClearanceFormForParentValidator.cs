using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Document.FnDocument.SendEmail;
using FluentValidation;

namespace BinusSchool.Document.FnDocument.SendEmail.Validator
{
    public class SendEmailClearanceFormForParentValidator : AbstractValidator<SendEmailClearanceFormForParentRequest>
    {
        public SendEmailClearanceFormForParentValidator()
        {
            RuleFor(x => x.IdSchool)
                .NotEmpty();

            RuleFor(x => x.IdSurveyPeriod)
                .NotEmpty();

            //RuleFor(x => x.IdClearanceWeekPeriod)
            //    .NotEmpty();

            RuleFor(x => x.StudentSurveyData)
                .NotNull();

            RuleFor(x => x.BLPFinalStatus)
                .NotNull();
        }
    }
}

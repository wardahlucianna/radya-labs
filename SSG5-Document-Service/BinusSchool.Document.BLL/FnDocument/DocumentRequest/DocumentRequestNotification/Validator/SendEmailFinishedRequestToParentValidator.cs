using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestNotification;
using FluentValidation;

namespace BinusSchool.Document.FnDocument.DocumentRequest.DocumentRequestNotification.Validator
{
    public class SendEmailFinishedRequestToParentValidator : AbstractValidator<SendEmailFinishedRequestToParentRequest>
    {
        public SendEmailFinishedRequestToParentValidator()
        {
            RuleFor(x => x.IdSchool).NotEmpty();
            RuleFor(x => x.IdDocumentReqApplicant).NotEmpty();
            RuleFor(x => x.IdStudent).NotEmpty();
        }
    }
}

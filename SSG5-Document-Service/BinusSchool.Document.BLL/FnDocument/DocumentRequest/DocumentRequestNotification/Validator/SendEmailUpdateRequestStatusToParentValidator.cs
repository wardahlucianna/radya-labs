using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestNotification;
using FluentValidation;

namespace BinusSchool.Document.FnDocument.DocumentRequest.DocumentRequestNotification.Validator
{
    public class SendEmailUpdateRequestStatusToParentValidator : AbstractValidator<SendEmailUpdateRequestStatusToParentRequest>
    {
        public SendEmailUpdateRequestStatusToParentValidator()
        {
            RuleFor(x => x.IdSchool).NotEmpty();
            RuleFor(x => x.IdDocumentReqApplicant).NotEmpty();
            RuleFor(x => x.IdStudent).NotEmpty();
        }
    }
}

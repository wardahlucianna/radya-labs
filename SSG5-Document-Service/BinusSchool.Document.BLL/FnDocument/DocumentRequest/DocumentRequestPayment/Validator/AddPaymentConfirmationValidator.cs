using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestPayment;
using FluentValidation;

namespace BinusSchool.Document.FnDocument.DocumentRequest.DocumentRequestPayment.Validator
{
    public class AddPaymentConfirmationValidator : AbstractValidator<AddPaymentConfirmationRequest>
    {
        public AddPaymentConfirmationValidator()
        {
            RuleFor(x => x.IdDocumentReqApplicant).NotEmpty();
            RuleFor(x => x.PaidAmount).NotEmpty();
            RuleFor(x => x.PaymentDate).NotEmpty();
            RuleFor(x => x.IdDocumentReqPaymentMethod).NotEmpty();
            RuleFor(x => x.SenderAccountName).NotEmpty();
        }
    }
}

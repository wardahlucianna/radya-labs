using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestPayment;
using FluentValidation;

namespace BinusSchool.Document.FnDocument.DocumentRequest.DocumentRequestPayment.Validator
{
    public class SavePaymentApprovalValidator : AbstractValidator<SavePaymentApprovalRequest>
    {
        public SavePaymentApprovalValidator()
        {
            RuleFor(x => x.IdDocumentReqApplicant).NotEmpty();
            RuleFor(x => x.PaymentDate).NotEmpty();
            RuleFor(x => x.IdDocumentReqPaymentMethod).NotEmpty();
            RuleFor(x => x.PaidAmount).NotNull();
            RuleFor(x => x.SenderAccountName).NotEmpty();
            RuleFor(x => x.VerificationStatus).NotNull();
            RuleFor(x => x.Remarks).NotEmpty().When(x => x.VerificationStatus == false).WithMessage("Please fill the remarks first");
        }
    }
}

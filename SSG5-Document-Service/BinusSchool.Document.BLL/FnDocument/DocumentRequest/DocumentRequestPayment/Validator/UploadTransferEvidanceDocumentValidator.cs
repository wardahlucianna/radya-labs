using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestPayment;
using FluentValidation;

namespace BinusSchool.Document.FnDocument.DocumentRequest.DocumentRequestPayment.Validator
{
    public class UploadTransferEvidanceDocumentValidator : AbstractValidator<UploadTransferEvidanceDocumentRequest>
    {
        public UploadTransferEvidanceDocumentValidator()
        {
            RuleFor(x => x.IdStudent).NotEmpty();
        }
    }
}

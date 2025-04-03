using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.MasterDocumentRequest;
using FluentValidation;

namespace BinusSchool.Document.FnDocument.DocumentRequest.MasterDocumentRequest.Validator
{
    public class DeleteSoftCopyDocumentValidator : AbstractValidator<DeleteSoftCopyDocumentRequest>
    {
        public DeleteSoftCopyDocumentValidator()
        {
            RuleFor(x => x.IdDocumentReqApplicantDetail).NotEmpty();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.MasterDocumentRequest;
using FluentValidation;

namespace BinusSchool.Document.FnDocument.DocumentRequest.MasterDocumentRequest.Validator
{
    public class SaveSoftCopyDocumentValidator : AbstractValidator<SaveSoftCopyDocumentRequest>
    {
        public SaveSoftCopyDocumentValidator()
        {
            RuleFor(x => x.IdDocumentReqApplicantDetail).NotEmpty();
            RuleFor(x => x.ShowToParent).NotNull();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.MasterDocumentRequest;
using FluentValidation;

namespace BinusSchool.Document.FnDocument.DocumentRequest.MasterDocumentRequest.Validator
{
    public class SaveFinishAndCollectReqDocumentValidator : AbstractValidator<SaveFinishAndCollectReqDocumentRequest>
    {
        public SaveFinishAndCollectReqDocumentValidator()
        {
            RuleFor(x => x.IdDocumentReqApplicant).NotEmpty();
            RuleFor(x => x.CollectedBy).NotEmpty().When(x => x.CollectedDate != null);
        }
    }
}

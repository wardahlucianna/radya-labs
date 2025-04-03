using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestHistory;
using FluentValidation;

namespace BinusSchool.Document.FnDocument.DocumentRequest.DocumentRequestHistory.Validator
{
    public class CancelDocumentRequestByParentValidator : AbstractValidator<CancelDocumentRequestByParentRequest>
    {
        public CancelDocumentRequestByParentValidator()
        {
            RuleFor(x => x.IdDocumentReqApplicant).NotEmpty();
        }
    }
}

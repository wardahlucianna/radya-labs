using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestApprover;
using FluentValidation;

namespace BinusSchool.Document.FnDocument.DocumentRequest.Validator
{
    public class RemoveDocumentRequestApproverValidator : AbstractValidator<RemoveDocumentRequestApproverRequest>
    {
        public RemoveDocumentRequestApproverValidator()
        {
            RuleFor(x => x.IdDocumentReqApprover).NotEmpty();
        }
    }
}

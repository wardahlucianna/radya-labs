using System;
using System.Collections.Generic;
using BinusSchool.Data.Model.Document.FnDocument.Document;
using FluentValidation;

namespace BinusSchool.Document.FnDocument.Document.Validator
{
    public class DocumentApprovalValidation : AbstractValidator<ApprovalDocumentRequest>
    {
        public DocumentApprovalValidation()
        {
            RuleFor(x => x.IdFormState)
               .NotEmpty();

            RuleFor(x => x.Action)
              .NotEmpty();

        }
    }
}

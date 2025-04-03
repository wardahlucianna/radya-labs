using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestApprover;
using FluentValidation;

namespace BinusSchool.Document.FnDocument.DocumentRequest.DocumentRequestApprover.Validator
{
    public class AddDocumentRequestApproverValidator : AbstractValidator<AddDocumentRequestApproverRequest>
    {
        public AddDocumentRequestApproverValidator()
        {
            RuleFor(x => x.IdSchool).NotEmpty();
            RuleFor(x => x.IdBinusian).NotEmpty();
        }
    }
}

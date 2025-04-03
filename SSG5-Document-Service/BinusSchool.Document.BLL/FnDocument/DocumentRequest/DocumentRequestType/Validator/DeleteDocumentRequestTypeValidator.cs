using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestType;
using FluentValidation;

namespace BinusSchool.Document.FnDocument.DocumentRequest.DocumentRequestType.Validator
{
    public class DeleteDocumentRequestTypeValidator : AbstractValidator<DeleteDocumentRequestTypeRequest>
    {
        public DeleteDocumentRequestTypeValidator()
        {
            RuleFor(x => x.IdDocumentReqType).NotEmpty();
        }
    }
}

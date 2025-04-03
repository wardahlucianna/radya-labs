using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestType;
using FluentValidation;

namespace BinusSchool.Document.FnDocument.DocumentRequest.DocumentRequestType.Validator
{
    public class UpdateDocumentTypeStatusValidator : AbstractValidator<UpdateDocumentTypeStatusRequest>
    {
        public UpdateDocumentTypeStatusValidator()
        {
            RuleFor(x => x.IdDocumentReqType).NotEmpty();
            RuleFor(x => x.ActiveStatus).NotNull();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestType;
using FluentValidation;

namespace BinusSchool.Document.FnDocument.DocumentRequest.DocumentRequestType.Validator
{
    public class UpdateImportedDataOptionCategoryValidator : AbstractValidator<UpdateImportedDataOptionCategoryRequest>
    {
        public UpdateImportedDataOptionCategoryValidator()
        {
            RuleFor(x => x.IdDocumentReqOptionCategory).NotEmpty();
        }
    }
}

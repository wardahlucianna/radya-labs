using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestType;
using FluentValidation;

namespace BinusSchool.Document.FnDocument.DocumentRequest.DocumentRequestType.Validator
{
    public class ExportExcelDocumentTypeListValidator : AbstractValidator<ExportExcelDocumentTypeListRequest>
    {
        public ExportExcelDocumentTypeListValidator()
        {
            RuleFor(x => x.IdSchool).NotEmpty();
        }
    }
}

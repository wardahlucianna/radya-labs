using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.MasterDocumentRequest;
using FluentValidation;

namespace BinusSchool.Document.FnDocument.DocumentRequest.MasterDocumentRequest.Validator
{
    public class ExportExcelGetDocumentRequestListValidator : AbstractValidator<ExportExcelGetDocumentRequestListRequest>
    {
        public ExportExcelGetDocumentRequestListValidator()
        {
            RuleFor(x => x.IdSchool).NotEmpty();
            RuleFor(x => x.RequestYear).NotNull();
        }
    }
}

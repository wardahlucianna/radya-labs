using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.MasterDocumentRequest;
using FluentValidation;

namespace BinusSchool.Document.FnDocument.DocumentRequest.MasterDocumentRequest.Validator
{
    public class CancelDocumentRequestByStaffValidator : AbstractValidator<CancelDocumentRequestByStaffRequest>
    {
        public CancelDocumentRequestByStaffValidator()
        {
            RuleFor(x => x.IdDocumentReqApplicant).NotEmpty();
            RuleFor(x => x.Remarks).NotEmpty();
        }
    }
}

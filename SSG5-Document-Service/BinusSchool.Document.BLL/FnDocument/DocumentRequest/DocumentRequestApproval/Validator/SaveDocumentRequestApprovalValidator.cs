using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestApproval;
using FluentValidation;

namespace BinusSchool.Document.FnDocument.DocumentRequest.DocumentRequestApproval.Validator
{
    public class SaveDocumentRequestApprovalValidator : AbstractValidator<SaveDocumentRequestApprovalRequest>
    {
        public SaveDocumentRequestApprovalValidator()
        {
            RuleFor(x => x.IdDocumentReqApplicant).NotEmpty();
            RuleFor(x => x.ApprovalStatus).NotNull();
            RuleFor(x => x.Remarks).NotEmpty().When(x => x.ApprovalStatus == DocumentRequestApprovalStatus.Declined).WithMessage("Please fill the remarks field");
        }
    }
}

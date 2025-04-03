using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestWorkflow;
using FluentValidation;

namespace BinusSchool.Document.FnDocument.DocumentRequest.DocumentRequestWorkflow.Validator
{
    public class AddDocumentRequestWorkflowValidator : AbstractValidator<AddDocumentRequestWorkflowRequest>
    {
        public AddDocumentRequestWorkflowValidator()
        {
            RuleFor(x => x.IdDocumentReqApplicant).NotEmpty();
            RuleFor(x => x.IdDocumentReqStatusWorkflow).NotEmpty();

            When(x => x.IdDocumentReqStatusWorkflow == DocumentRequestStatusWorkflow.Declined
                        || x.IdDocumentReqStatusWorkflow == DocumentRequestStatusWorkflow.Canceled, () =>
            {
                RuleFor(x => x.Remarks).NotEmpty();
            });
        }
    }
}

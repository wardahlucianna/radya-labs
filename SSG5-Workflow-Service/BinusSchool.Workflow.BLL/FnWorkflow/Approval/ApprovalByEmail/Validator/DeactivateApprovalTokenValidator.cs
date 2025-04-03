using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Workflow.FnWorkflow.Approval.ApprovalByEmail;
using FluentValidation;

namespace BinusSchool.Workflow.FnWorkflow.Approval.ApprovalByEmail.Validator
{
    public class DeactivateApprovalTokenValidator : AbstractValidator<DeactivateApprovalTokenRequest>
    {
        public DeactivateApprovalTokenValidator()
        {
            RuleFor(x => x.IdApprovalToken).NotEmpty();
        }
    }
}

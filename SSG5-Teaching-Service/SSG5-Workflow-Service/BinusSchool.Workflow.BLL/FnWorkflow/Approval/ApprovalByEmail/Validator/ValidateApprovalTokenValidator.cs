using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Workflow.FnWorkflow.Approval.ApprovalByEmail;
using FluentValidation;

namespace BinusSchool.Workflow.FnWorkflow.Approval.ApprovalByEmail.Validator
{
    public class ValidateApprovalTokenValidator : AbstractValidator<ValidateApprovalTokenRequest>
    {
        public ValidateApprovalTokenValidator()
        {
            RuleFor(x => x.ActionKey).NotEmpty();
        }
    }
}

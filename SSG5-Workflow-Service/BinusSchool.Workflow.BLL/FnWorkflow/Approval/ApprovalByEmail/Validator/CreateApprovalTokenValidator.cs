using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Workflow.FnWorkflow.Approval.ApprovalByEmail;
using FluentValidation;

namespace BinusSchool.Workflow.FnWorkflow.Approval.ApprovalByEmail.Validator
{
    public class CreateApprovalTokenValidator : AbstractValidator<CreateApprovalTokenRequest>
    {
        public CreateApprovalTokenValidator()
        {
            RuleFor(x => x.IdTransaction).NotEmpty();
            RuleFor(x => x.Module).NotEmpty();
        }
    }
}

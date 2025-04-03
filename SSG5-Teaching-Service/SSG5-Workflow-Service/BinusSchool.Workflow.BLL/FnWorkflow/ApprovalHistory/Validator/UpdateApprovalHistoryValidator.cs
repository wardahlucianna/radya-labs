using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Workflow.FnWorkflow.Approval;
using FluentValidation;

namespace BinusSchool.Workflow.FnWorkflow.ApprovalHistory.Validator
{
    public class UpdateApprovalHistoryValidator : AbstractValidator<UpdateApprovalHistoryRequest>
    {
        public UpdateApprovalHistoryValidator()
        {
            Include(new AddApprovalHistoryValidator());
            RuleFor(x => x.Id).NotEmpty();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Workflow.FnWorkflow.Approval;
using FluentValidation;

namespace BinusSchool.Workflow.FnWorkflow.ApprovalHistory.Validator
{
    public class UpdateTaskhistoryValidator : AbstractValidator<UpdateTaskHistoryRequest>
    {
        public UpdateTaskhistoryValidator()
        {
            RuleFor(x => x.Action).NotEmpty();
            RuleFor(x => x.UserID).NotEmpty();
        }
    }
}

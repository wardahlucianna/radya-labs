using BinusSchool.Data.Model.Workflow.FnWorkflow.Approval;
using FluentValidation;

namespace BinusSchool.Workflow.FnWorkflow.ApprovalHistory.Validator
{
    public class AddApprovalHistoryValidator : AbstractValidator<AddApprovalHistoryRequest>
    {
        public AddApprovalHistoryValidator()
        {
            RuleFor(x => x.IdDocument).NotEmpty();
            RuleFor(x => x.IdUserAction).NotEmpty();
            RuleFor(x => x.IdFormState).NotEmpty();
        }
    }
}

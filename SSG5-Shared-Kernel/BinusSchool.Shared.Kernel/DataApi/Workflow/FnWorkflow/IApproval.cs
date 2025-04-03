using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Workflow.FnWorkflow.Approval;
using BinusSchool.Data.Model.Workflow.FnWorkflow.Approval.ApprovalByEmail;
using Refit;

namespace BinusSchool.Data.Api.Workflow.FnWorkflow
{
    public interface IApproval: IFnWorkflow
    {
        [Get("/workflow/approval-state-by-workflow")]
        Task<ApiErrorResult<GetApprovalStateByWorkflowResult>> GetApprovalStateByWorkflow([Query] GetApprovalStateByWorkflowRequest param);

        [Get("/workflow/approval-state-by-workflow-without")]
        Task<ApiErrorResult<List<GetApprovalStateByWorkflowResult>>> GetListApprovalStateByWorkflowWithout([Query] GetListApprovalStateWithWorkflowRequest param);

        [Get("/workflow/approval-state-in-approve")]
        Task<ApiErrorResult<GetInApproveResult>> GetInApprove([Query] GetInApproveRequest param);

        [Post("/workflow/approval-state-create-approval-token")]
        Task<ApiErrorResult<CreateApprovalTokenResult>> CreateApprovalToken(CreateApprovalTokenRequest query);

        [Post("/workflow/approval-state-validate-approval-token")]
        Task<ApiErrorResult<ValidateApprovalTokenResult>> ValidateApprovalToken(ValidateApprovalTokenRequest query);

        [Post("/workflow/approval-state-deactivate-approval-token")]
        Task<ApiErrorResult> DeactivateApprovalToken(DeactivateApprovalTokenRequest query);
    }
}

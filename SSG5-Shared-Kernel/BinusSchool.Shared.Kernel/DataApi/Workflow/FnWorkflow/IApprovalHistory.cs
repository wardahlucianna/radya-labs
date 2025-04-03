using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Workflow.FnWorkflow.Approval;
using Refit;

namespace BinusSchool.Data.Api.Workflow.FnWorkflow
{
    public interface IApprovalHistory : IFnWorkflow
    {
        [Post("/workflow/approval-history")]
        Task<ApiErrorResult> AddApprovalHistory([Body] AddApprovalHistoryRequest body);

        [Put("/workflow/approval-history")]
        Task<ApiErrorResult> UpdateApprovalHistory([Body] UpdateApprovalHistoryRequest body);

        [Put("/workflow/approval-history-update-task")]
        Task<ApiErrorResult> UpdateTaskApprovalHistory([Body] UpdateTaskHistoryRequest body);

        [Get("/workflow/approval-history-by-user")]
        Task<ApiErrorResult<GetApprovalHistoryByUserResult>> GetApprovalHistoryByUser([Query] GetApprovalHistoryByUserRequest request);
    }
}

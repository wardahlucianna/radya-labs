using System;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.Workflow.FnWorkflow.Approval;
using BinusSchool.Persistence.WorkflowDb.Abstractions;
using BinusSchool.Persistence.WorkflowDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Workflow.FnWorkflow.ApprovalHistory
{
    public class GetApprovalHistoryByUserHanlder : FunctionsHttpSingleHandler
    {
        private readonly IWorkflowDbContext _dbContext;
        public GetApprovalHistoryByUserHanlder(IWorkflowDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetApprovalHistoryByUserRequest>(nameof(GetApprovalHistoryByUserRequest.IdUserAction));
            var getApproval = await _dbContext.Entity<MsApprovalHistory>()
                                              .Where(p => p.IdUserAction == param.IdUserAction &&
                                                          p.IdDocument == param.IdDocument &&
                                                          p.Action==param.Action)
                                              .Select(p => new GetApprovalHistoryByUserResult
                                              {
                                                  IdForUser = p.IdUserAction,
                                                  IdFromState = p.IdFormState,
                                                  IdApprovalTask = p.Id,
                                                  IdDocument = p.IdDocument,
                                                  Action = p.Action == ApprovalStatus.Draft?"":p.Action.ToString(),
                                              })
                                              .FirstOrDefaultAsync();
            if (getApproval is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["SchApprovalState"], "Id", param.IdDocument));


            return Request.CreateApiResult2(getApproval as object);
        }
    }
}

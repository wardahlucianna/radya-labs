using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Data.Model.Workflow.FnWorkflow.Approval;
using BinusSchool.Persistence.WorkflowDb.Abstractions;
using BinusSchool.Persistence.WorkflowDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Workflow.FnWorkflow.Approval
{
    public class GetApprovalStateHandler : FunctionsHttpSingleHandler
    {
        private readonly IWorkflowDbContext _dbContext;
        public GetApprovalStateHandler(IWorkflowDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetApprovalStateByWorkflowRequest>(nameof(GetApprovalStateByWorkflowRequest.IdApprovalWorkflow));
            var getApproval = await _dbContext.Entity<MsApprovalState>()
                                              .Include(p => p.ApprovalWorkflow)
                                              .Where(p => p.StateType == param.StateType && p.IdApprovalWorkflow == param.IdApprovalWorkflow)
                                              .Select(p=> new GetApprovalStateByWorkflowResult 
                                              {
                                                Id=p.Id,
                                                StateName=p.StateName,
                                                StateNumber=p.StateNumber,
                                                StateType=p.StateType,
                                                IdApprovalWorkflow=p.IdApprovalWorkflow,
                                                IdRole=p.IdRole,
                                              })
                                              .FirstOrDefaultAsync();
            if (getApproval is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["SchApprovalState"], "Id", param.IdApprovalWorkflow));


            return Request.CreateApiResult2(getApproval as object);
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Workflow.FnWorkflow.Approval;
using BinusSchool.Persistence.WorkflowDb.Abstractions;
using BinusSchool.Persistence.WorkflowDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Workflow.FnWorkflow.Approval
{
    public class GetListApprovalStateWithWorkflowHandler : FunctionsHttpSingleHandler
    {
        private readonly IWorkflowDbContext _dbContext;
        public GetListApprovalStateWithWorkflowHandler(IWorkflowDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetListApprovalStateWithWorkflowRequest>(new string[] { nameof(GetListApprovalStateWithWorkflowRequest.IdWorkflow), nameof(GetListApprovalStateWithWorkflowRequest.WithoutState) });
            var getApproveState = await _dbContext.Entity<MsApprovalState>()
                                                .Where(p => p.StateType != param.WithoutState && p.IdApprovalWorkflow == param.IdWorkflow)
                                                .Select(p => new GetApprovalStateByWorkflowResult
                                                {
                                                    Id = p.Id,
                                                    StateName = p.StateName,
                                                    StateNumber = p.StateNumber,
                                                    StateType = p.StateType,
                                                    IdApprovalWorkflow = p.IdApprovalWorkflow,
                                                    IdRole = p.IdRole,
                                                }).ToListAsync();

            if (getApproveState is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["SchApprovalState"], "Id", param.IdWorkflow));


            return Request.CreateApiResult2(getApproveState as object);
        }
    }
}

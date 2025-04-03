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
using BinusSchool.Common.Model.Enums;
namespace BinusSchool.Workflow.FnWorkflow.Approval
{
    public class GetInApproveHandler : FunctionsHttpSingleHandler
    {
        private readonly IWorkflowDbContext _dbContext;
        public GetInApproveHandler(IWorkflowDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetInApproveRequest>(new string[] { nameof(GetInApproveRequest.IdFromState), nameof(GetInApproveRequest.Action) });
            var getInApprove = await _dbContext.Entity<MsApprovalTransition>()
                                                .Include(p => p.FromState).ThenInclude(p => p.FromTransitions)
                                                .Include(p => p.ToState).ThenInclude(p => p.ToTransitions)
                                                .Where(p => p.Action == (ApprovalAction)param.Action && p.IdFromState == param.IdFromState)
                                                .Select(p=> new GetInApproveResult 
                                                {
                                                    IdFromState=p.IdFromState,
                                                    IdToState=p.IdToState,
                                                    FromStateIdRoleAction=p.FromState.IdRole,
                                                    ToStateIdRoleAction=p.ToState.IdRole,
                                                    ToStateType=p.ToState.StateType,
                                                    FromStateType=p.FromState.StateType,
                                                    Status=p.Status,
                                                })
                                                .FirstOrDefaultAsync();

            if (getInApprove is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["SchApprovalState"], "Id", param.IdFromState));


            return Request.CreateApiResult2(getInApprove as object);
        }
    }
}

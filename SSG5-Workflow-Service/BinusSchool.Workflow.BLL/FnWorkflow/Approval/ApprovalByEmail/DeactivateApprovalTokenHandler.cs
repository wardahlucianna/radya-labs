using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Workflow.FnWorkflow.Approval.ApprovalByEmail;
using BinusSchool.Persistence.WorkflowDb.Abstractions;
using BinusSchool.Persistence.WorkflowDb.Entities;
using BinusSchool.Workflow.FnWorkflow.Approval.ApprovalByEmail.Validator;

namespace BinusSchool.Workflow.FnWorkflow.Approval.ApprovalByEmail
{
    public class DeactivateApprovalTokenHandler : FunctionsHttpSingleHandler
    {
        private readonly IWorkflowDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;

        public DeactivateApprovalTokenHandler(IWorkflowDbContext dbContext, IMachineDateTime dateTime)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
        }

        protected async override Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<DeactivateApprovalTokenRequest, DeactivateApprovalTokenValidator>();

            var approvalToken = _dbContext.Entity<TrApprovalToken>()
                                    .Where(x => x.IdApprovalToken == param.IdApprovalToken)
                                    .FirstOrDefault();

            if(approvalToken != null)
            {
                // Update TrApprovalToken
                approvalToken.StatusToken = false;
                approvalToken.IpAddress = param.IpAddress;
                approvalToken.MacAddress = param.MacAddress;
                approvalToken.ActionDate = _dateTime.ServerTime;

                _dbContext.Entity<TrApprovalToken>().Update(approvalToken);
                await _dbContext.SaveChangesAsync();
            }

            throw new BadRequestException("Approval token not found.");
        }
    }
}

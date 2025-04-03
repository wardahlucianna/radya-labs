using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Workflow.FnWorkflow.Approval.ApprovalByEmail;
using BinusSchool.Persistence.WorkflowDb.Abstractions;
using BinusSchool.Persistence.WorkflowDb.Entities;
using BinusSchool.Workflow.FnWorkflow.Approval.ApprovalByEmail.Validator;
using BinusSchool.Workflow.FnWorkflow.Helper;
using Microsoft.Extensions.Configuration;

namespace BinusSchool.Workflow.FnWorkflow.SendEmail.ApprovalByEmail
{
    public class CreateApprovalTokenHandler : FunctionsHttpSingleHandler
    {
        private readonly IWorkflowDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly IMachineDateTime _dateTime;
        public CreateApprovalTokenHandler(IWorkflowDbContext dbContext, IConfiguration configuration, IMachineDateTime dateTime)
        {
            _dbContext = dbContext;
            _configuration = configuration;
            _dateTime = dateTime;
        }

        protected async override Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<CreateApprovalTokenRequest, CreateApprovalTokenValidator>();

            string idApprovalToken = Guid.NewGuid().ToString();

            // set expired date
            int tokenLimitDays = 7;  // days after creation
            var expiredDate = _dateTime.ServerTime.AddDays(tokenLimitDays);

            // input to TrApprovalToken
            var newApprovalToken = new TrApprovalToken
            {
                IdApprovalToken = idApprovalToken,
                ExpiredDate = expiredDate,
                StatusToken = true,
                Module = param.Module,
                IdTransaction = param.IdTransaction
            };
            _dbContext.Entity<TrApprovalToken>().Add(newApprovalToken);

            // Encrypt key => key = {IdApprovalToken}*={Approve/Reject}*={ApprovalKey}
            var approvalKey = _configuration.GetSection("ApprovalKey:Key").Get<string>();
            var approvalSeparator = _configuration.GetSection("ApprovalKey:Separator").Get<string>();

            var approveActionKey = EncryptString.Encrypt((
                idApprovalToken + approvalSeparator + 
                ApprovalActionConstant.Approve + approvalSeparator + 
                approvalKey));

            var rejectActionKey = EncryptString.Encrypt((
                idApprovalToken + approvalSeparator +
                ApprovalActionConstant.Reject + approvalSeparator + 
                approvalKey));

            // Kirim hasil return ke API SendEmailApproval

            CreateApprovalTokenResult result = new CreateApprovalTokenResult
            {
                ApproveActionKey = approveActionKey,
                RejectActionKey = rejectActionKey,
                ExpiredDate = expiredDate
            };

            await _dbContext.SaveChangesAsync(CancellationToken);
            return Request.CreateApiResult2(result as object);
        }
    }
}

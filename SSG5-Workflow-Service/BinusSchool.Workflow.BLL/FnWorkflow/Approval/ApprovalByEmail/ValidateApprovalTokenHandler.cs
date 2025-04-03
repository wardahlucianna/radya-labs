using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Abstractions;
using BinusSchool.Data.Api.Extensions;
using BinusSchool.Data.Model.Workflow.FnWorkflow.Approval.ApprovalByEmail;
using BinusSchool.Persistence.WorkflowDb.Abstractions;
using BinusSchool.Persistence.WorkflowDb.Entities;
using BinusSchool.Workflow.FnWorkflow.Approval.ApprovalByEmail.Validator;
using BinusSchool.Workflow.FnWorkflow.Helper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;

namespace BinusSchool.Workflow.FnWorkflow.SendEmail.ApprovalByEmail
{
    public class ValidateApprovalTokenHandler : FunctionsHttpSingleHandler
    {
        private readonly IWorkflowDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        private readonly IConfiguration _configuration;
        public ValidateApprovalTokenHandler(IWorkflowDbContext dbContext, IConfiguration configuration, IMachineDateTime dateTime)
        {
            _dbContext = dbContext;
            _configuration = configuration;
            _dateTime = dateTime;
        }
        protected async override Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<ValidateApprovalTokenRequest, ValidateApprovalTokenValidator>();

            var result = new ValidateApprovalTokenResult();

            try
            {
                // decrypt ActionKey
                var decryptedActionKey = EncryptString.Decrypt(param.ActionKey);

                // split ActionKey
                var approvalKeyReal = _configuration.GetSection("ApprovalKey:Key").Get<string>();
                var approvalSeparator = _configuration.GetSection("ApprovalKey:Separator").Get<string>();

                var splittedActionKey = decryptedActionKey.Split(approvalSeparator);
                var idApprovalToken = splittedActionKey[0];
                var actionString = splittedActionKey[1];

                var approvalKey = splittedActionKey[2];

                // validasi approval key
                if (approvalKey == approvalKeyReal)
                {
                    // validasi idApprovalToken and expired date
                    var approvalToken = await _dbContext.Entity<TrApprovalToken>()
                                        .Where(x => x.IdApprovalToken == idApprovalToken)
                                        .FirstOrDefaultAsync(CancellationToken);

                    if (approvalToken != null)
                    {

                        if (approvalToken.StatusToken == false)
                        {
                            result = new ValidateApprovalTokenResult
                            {
                                IsValid = false,
                                IdApprovalToken = null,
                                IdTransaction = null,
                                Module = null,
                                Action = null,
                                ReturnMessage = string.Format("Approval request has already been approved or rejected on {0}.", approvalToken.ActionDate == null ? "-" : approvalToken.ActionDate.Value.ToString("dd MMMM yyyy HH:mm:ss")),
                                ShowAsError = false
                            };

                            return Request.CreateApiResult2(result as object);
                        }

                        if (_dateTime.ServerTime > approvalToken.ExpiredDate)
                        {
                            result = new ValidateApprovalTokenResult
                            {
                                IsValid = false,
                                IdApprovalToken = null,
                                IdTransaction = null,
                                Module = null,
                                Action = null,
                                ReturnMessage = string.Format("Approval request has exceeded the expiration time."),
                                ShowAsError = false
                            };

                            return Request.CreateApiResult2(result as object);
                        }


                        if (approvalToken.StatusToken && _dateTime.ServerTime <= approvalToken.ExpiredDate)
                        {
                            ApprovalStatus action = new ApprovalStatus();
                            if (actionString == ApprovalActionConstant.Approve) action = ApprovalStatus.Approved;
                            else if (actionString == ApprovalActionConstant.Reject) action = ApprovalStatus.Reject;

                            result = new ValidateApprovalTokenResult
                            {
                                IsValid = true,
                                IdApprovalToken = approvalToken.IdApprovalToken,
                                IdTransaction = approvalToken.IdTransaction,
                                Module = approvalToken.Module,
                                Action = action,
                                ReturnMessage = null,
                                ShowAsError = false
                            };

                            return Request.CreateApiResult2(result as object);
                        }
                    }
                    else
                    {
                        result = new ValidateApprovalTokenResult
                        {
                            IsValid = false,
                            IdApprovalToken = null,
                            IdTransaction = null,
                            Module = null,
                            Action = null,
                            ReturnMessage = string.Format("You are not authorized to perform this specific action."),
                            ShowAsError = true
                        };

                        return Request.CreateApiResult2(result as object);
                    }
                }

                return Request.CreateApiResult2(result as object);
            }
            catch (Exception ex)
            {
                result = new ValidateApprovalTokenResult
                {
                    IsValid = false,
                    IdApprovalToken = null,
                    IdTransaction = null,
                    Module = null,
                    Action = null,
                    ReturnMessage = string.Format("Internal server error. Please contact the administrator."),
                    ShowAsError = true
                };

                return Request.CreateApiResult2(result as object);
            }
        }
    }
}

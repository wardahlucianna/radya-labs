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
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestNotification;
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestWorkflow;
using BinusSchool.Document.FnDocument.DocumentRequest.DocumentRequestNotification;
using BinusSchool.Document.FnDocument.DocumentRequest.DocumentRequestWorkflow.Validator;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.DocumentDb.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.Document.FnDocument.DocumentRequest.DocumentRequestWorkflow
{
    public class AddDocumentRequestWorkflowHandler : FunctionsHttpSingleHandler
    {
        private IDbContextTransaction _transaction;
        private readonly IDocumentDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        private readonly GetDateBusinessDaysByStartDateHandler _getDateBusinessDaysByStartDateHandler;
        private readonly SendEmailCancelRequestToParentHandler _sendEmailCancelRequestToParentHandler;
        private readonly SendEmailCancelRequestToStaffHandler _sendEmailCancelRequestToStaffHandler;
        private readonly SendEmailNeedProcessRequestToPICHandler _sendEmailNeedProcessRequestToPICHandler;
        private readonly SendEmailUpdateRequestStatusToParentHandler _sendEmailUpdateRequestStatusToParentHandler;
        private readonly SendEmailFinishedRequestToParentHandler _sendEmailFinishedRequestToParentHandler;
        private readonly SendEmailCollectedRequestToParentHandler _sendEmailCollectedRequestToParentHandler;
        private readonly SendEmailNeedApprovalRequestToApproverHandler _sendEmailNeedApprovalRequestToApproverHandler;
        private readonly SendEmailNeedPaymentVerificationRequestToApproverHandler _sendEmailNeedPaymentVerificationRequestToApproverHandler;

        public AddDocumentRequestWorkflowHandler(
            IDocumentDbContext dbContext,
            IMachineDateTime dateTime,
            GetDateBusinessDaysByStartDateHandler getDateBusinessDaysByStartDateHandler,
            SendEmailCancelRequestToParentHandler sendEmailCancelRequestToParentHandler,
            SendEmailCancelRequestToStaffHandler sendEmailCancelRequestToStaffHandler,
            SendEmailNeedProcessRequestToPICHandler sendEmailNeedProcessRequestToPICHandler,
            SendEmailUpdateRequestStatusToParentHandler sendEmailUpdateRequestStatusToParentHandler,
            SendEmailFinishedRequestToParentHandler sendEmailFinishedRequestToParentHandler,
            SendEmailCollectedRequestToParentHandler sendEmailCollectedRequestToParentHandler,
            SendEmailNeedApprovalRequestToApproverHandler sendEmailNeedApprovalRequestToApproverHandler,
            SendEmailNeedPaymentVerificationRequestToApproverHandler sendEmailNeedPaymentVerificationRequestToApproverHandler)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
            _getDateBusinessDaysByStartDateHandler = getDateBusinessDaysByStartDateHandler;
            _sendEmailCancelRequestToParentHandler = sendEmailCancelRequestToParentHandler;
            _sendEmailCancelRequestToStaffHandler = sendEmailCancelRequestToStaffHandler;
            _sendEmailNeedProcessRequestToPICHandler = sendEmailNeedProcessRequestToPICHandler;
            _sendEmailUpdateRequestStatusToParentHandler = sendEmailUpdateRequestStatusToParentHandler;
            _sendEmailFinishedRequestToParentHandler = sendEmailFinishedRequestToParentHandler;
            _sendEmailCollectedRequestToParentHandler = sendEmailCollectedRequestToParentHandler;
            _sendEmailNeedApprovalRequestToApproverHandler = sendEmailNeedApprovalRequestToApproverHandler;
            _sendEmailNeedPaymentVerificationRequestToApproverHandler = sendEmailNeedPaymentVerificationRequestToApproverHandler;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<AddDocumentRequestWorkflowRequest, AddDocumentRequestWorkflowValidator>();

            var result = await AddDocumentRequestWorkflow(new AddDocumentRequestWorkflowRequest
            {
                IdDocumentReqApplicant = param.IdDocumentReqApplicant,
                IdDocumentReqStatusWorkflow = param.IdDocumentReqStatusWorkflow,
                IdBinusianStaff = param.IdBinusianStaff,
                Remarks = param.Remarks
            });

            return Request.CreateApiResult2(result as object);
        }

        public async Task<AddDocumentRequestWorkflowResult> AddDocumentRequestWorkflow(AddDocumentRequestWorkflowRequest param)
        {
            var result = new AddDocumentRequestWorkflowResult();
            try
            {
                _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

                var statusDate = _dateTime.ServerTime;

                var getDocumentReqApplicant = await _dbContext.Entity<MsDocumentReqApplicant>()
                                                .FindAsync(param.IdDocumentReqApplicant);

                if (getDocumentReqApplicant == null)
                    throw new BadRequestException("Document request applicant data is not found");

                var getLastDocumentReqStatus = await _dbContext.Entity<TrDocumentReqStatusTrackingHistory>()
                                                .Where(x => x.IdDocumentReqApplicant == param.IdDocumentReqApplicant)
                                                .OrderByDescending(x => x.StatusDate)
                                                .FirstOrDefaultAsync(CancellationToken);

                bool isFirstRequestStatus = getLastDocumentReqStatus == null;
                bool lastStatusIsOnProcess = getLastDocumentReqStatus == null ? false : getLastDocumentReqStatus.IsOnProcess;

                var newIdDocumentReqStatusTrackingHistory = Guid.NewGuid().ToString();
                var newTrDocumentReqStatusTrackingHistory = new TrDocumentReqStatusTrackingHistory
                {
                    Id = newIdDocumentReqStatusTrackingHistory,
                    IdDocumentReqApplicant = param.IdDocumentReqApplicant,
                    IdDocumentReqStatusWorkflow = param.IdDocumentReqStatusWorkflow,
                    IdBinusianStaff = param.IdBinusianStaff,
                    IsOnProcess = (getDocumentReqApplicant.CanProcessBeforePaid && 
                        param.IdDocumentReqStatusWorkflow != DocumentRequestStatusWorkflow.Finished &&
                        param.IdDocumentReqStatusWorkflow != DocumentRequestStatusWorkflow.Collected &&
                        param.IdDocumentReqStatusWorkflow != DocumentRequestStatusWorkflow.Canceled && 
                        param.IdDocumentReqStatusWorkflow != DocumentRequestStatusWorkflow.Declined) ||
                        param.IdDocumentReqStatusWorkflow == DocumentRequestStatusWorkflow.OnProcess ? true : false,
                    StatusDate = statusDate,
                    Remarks = param.Remarks
                };

                _dbContext.Entity<TrDocumentReqStatusTrackingHistory>().Add(newTrDocumentReqStatusTrackingHistory);

                // condition when status is "ON PROCESS"
                // count estimation finish date
                if (newTrDocumentReqStatusTrackingHistory.IsOnProcess && getDocumentReqApplicant.EstimationFinishDate == null)
                {
                    var estimationFinishDate = await _getDateBusinessDaysByStartDateHandler.CountDateBusinessDaysByStartDate(new GetDateBusinessDaysByStartDateRequest
                    {
                        IdSchool = getDocumentReqApplicant.IdSchool,
                        StartDate = statusDate,
                        TotalDays = getDocumentReqApplicant.EstimationFinishDays,
                        CountHoliday = true
                    });

                    getDocumentReqApplicant.EstimationFinishDate = estimationFinishDate.EndDate;
                }

                getDocumentReqApplicant.IdDocumentReqStatusWorkflow = param.IdDocumentReqStatusWorkflow;
                _dbContext.Entity<MsDocumentReqApplicant>().Update(getDocumentReqApplicant);

                result.IsSuccess = true;
                result.IdDocumentReqStatusTrackingHistory = newIdDocumentReqStatusTrackingHistory;

                await _dbContext.SaveChangesAsync(CancellationToken);
                await _transaction.CommitAsync(CancellationToken);

                // Send email
                try
                {
                    // udpate request status (On Process / Declined / Waiting for Payment Verification)
                    if ((newTrDocumentReqStatusTrackingHistory.IdDocumentReqStatusWorkflow == DocumentRequestStatusWorkflow.OnProcess ||
                        newTrDocumentReqStatusTrackingHistory.IdDocumentReqStatusWorkflow == DocumentRequestStatusWorkflow.Declined ||
                        newTrDocumentReqStatusTrackingHistory.IdDocumentReqStatusWorkflow == DocumentRequestStatusWorkflow.WaitingForPaymentVerification ||
                        newTrDocumentReqStatusTrackingHistory.IdDocumentReqStatusWorkflow == DocumentRequestStatusWorkflow.WaitingForPayment) && !isFirstRequestStatus)
                    {
                        // send email to parent
                        await _sendEmailUpdateRequestStatusToParentHandler.SendEmailUpdateRequestStatusToParent(new SendEmailUpdateRequestStatusToParentRequest
                        {
                            IdSchool = getDocumentReqApplicant.IdSchool,
                            IdDocumentReqApplicant = param.IdDocumentReqApplicant,
                            IdStudent = getDocumentReqApplicant.IdStudent
                        });
                    }
                }
                catch (Exception ex)
                {

                }

                try
                {
                    // cancel request
                    if (newTrDocumentReqStatusTrackingHistory.IdDocumentReqStatusWorkflow == DocumentRequestStatusWorkflow.Canceled)
                    {
                        // send email to parent
                        await _sendEmailCancelRequestToParentHandler.SendEmailCancelRequestToParent(new SendEmailCancelRequestToParentRequest
                        {
                            IdSchool = getDocumentReqApplicant.IdSchool,
                            IdDocumentReqApplicant = param.IdDocumentReqApplicant,
                            IdStudent = getDocumentReqApplicant.IdStudent
                        });

                        // send email to staff
                        await _sendEmailCancelRequestToStaffHandler.SendEmailCancelRequestToStaff(new SendEmailCancelRequestToStaffRequest
                        {
                            IdSchool = getDocumentReqApplicant.IdSchool,
                            IdDocumentReqApplicant = param.IdDocumentReqApplicant,
                            IdStudent = getDocumentReqApplicant.IdStudent
                        });
                    }
                }
                catch (Exception ex)
                {

                }

                try
                {
                    // Email Need Process Document Request to PIC
                    if (newTrDocumentReqStatusTrackingHistory.IsOnProcess && lastStatusIsOnProcess == false)
                    {
                        // send email to PIC
                        await _sendEmailNeedProcessRequestToPICHandler.SendEmailNeedProcessRequestToPIC(new SendEmailNeedProcessRequestToPICRequest
                        {
                            IdSchool = getDocumentReqApplicant.IdSchool,
                            IdDocumentReqApplicant = param.IdDocumentReqApplicant,
                            IdStudent = getDocumentReqApplicant.IdStudent
                        });
                    }
                }
                catch (Exception ex)
                {

                }

                try
                {
                    // finished document
                    if (newTrDocumentReqStatusTrackingHistory.IdDocumentReqStatusWorkflow == DocumentRequestStatusWorkflow.Finished)
                    {
                        // send email to Parent
                        await _sendEmailFinishedRequestToParentHandler.SendEmailFinishedRequestToParent(new SendEmailFinishedRequestToParentRequest
                        {
                            IdSchool = getDocumentReqApplicant.IdSchool,
                            IdDocumentReqApplicant = param.IdDocumentReqApplicant,
                            IdStudent = getDocumentReqApplicant.IdStudent
                        });
                    }
                }
                catch (Exception ex)
                {

                }

                try
                {
                    // collected document
                    if (newTrDocumentReqStatusTrackingHistory.IdDocumentReqStatusWorkflow == DocumentRequestStatusWorkflow.Collected)
                    {
                        // send email to Parent
                        await _sendEmailCollectedRequestToParentHandler.SendEmailCollectedRequestToParent(new SendEmailCollectedRequestToParentRequest
                        {
                            IdSchool = getDocumentReqApplicant.IdSchool,
                            IdDocumentReqApplicant = param.IdDocumentReqApplicant,
                            IdStudent = getDocumentReqApplicant.IdStudent
                        });
                    }
                }
                catch (Exception ex)
                {

                }

                try
                {
                    // document need approval to approver
                    if (newTrDocumentReqStatusTrackingHistory.IdDocumentReqStatusWorkflow == DocumentRequestStatusWorkflow.WaitingForApproval)
                    {
                        // send email to Parent
                        await _sendEmailNeedApprovalRequestToApproverHandler.SendEmailNeedApprovalRequestToApprover(new SendEmailNeedApprovalRequestToApproverRequest
                        {
                            IdSchool = getDocumentReqApplicant.IdSchool,
                            IdDocumentReqApplicant = param.IdDocumentReqApplicant,
                            IdStudent = getDocumentReqApplicant.IdStudent
                        });
                    }
                }
                catch (Exception ex)
                {

                }

                try
                {
                    // document need payment verification to approver
                    if (newTrDocumentReqStatusTrackingHistory.IdDocumentReqStatusWorkflow == DocumentRequestStatusWorkflow.WaitingForPaymentVerification)
                    {
                        // send email to Parent
                        await _sendEmailNeedPaymentVerificationRequestToApproverHandler.SendEmailNeedPaymentVerificationRequestToApprover(new SendEmailNeedPaymentVerificationRequestToApproverRequest
                        {
                            IdSchool = getDocumentReqApplicant.IdSchool,
                            IdDocumentReqApplicant = param.IdDocumentReqApplicant,
                            IdStudent = getDocumentReqApplicant.IdStudent
                        });
                    }
                }
                catch (Exception ex)
                {

                }
            }
            catch (Exception ex)
            {
                _transaction?.Rollback();

                result.IsSuccess = false;
                result.ErrorMessage = ex.ToString();
            }
            finally
            {
                _transaction?.Dispose();
            }

            return result;
        }
    }
}

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
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestApproval;
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestApprover;
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestWorkflow;
using BinusSchool.Document.FnDocument.DocumentRequest.DocumentRequestApproval.Validator;
using BinusSchool.Document.FnDocument.DocumentRequest.DocumentRequestApprover;
using BinusSchool.Document.FnDocument.DocumentRequest.DocumentRequestWorkflow;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.DocumentDb.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.Document.FnDocument.DocumentRequest.DocumentRequestApproval
{
    public class SaveDocumentRequestApprovalHandler : FunctionsHttpSingleHandler
    {
        private readonly IDocumentDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        private readonly AddDocumentRequestWorkflowHandler _addDocumentRequestWorkflowHandler;
        private readonly CheckAdminAccessByIdBinusianHandler _checkAdminAccessByIdBinusianHandler;
        private IDbContextTransaction _transaction;

        public SaveDocumentRequestApprovalHandler(
            IDocumentDbContext dbContext,
            IMachineDateTime dateTime,
            AddDocumentRequestWorkflowHandler addDocumentRequestWorkflowHandler,
            CheckAdminAccessByIdBinusianHandler checkAdminAccessByIdBinusianHandler)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
            _addDocumentRequestWorkflowHandler = addDocumentRequestWorkflowHandler;
            _checkAdminAccessByIdBinusianHandler = checkAdminAccessByIdBinusianHandler;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<SaveDocumentRequestApprovalRequest, SaveDocumentRequestApprovalValidator>();

            try
            {
                _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

                var getDocumentReqApplicant = await _dbContext.Entity<MsDocumentReqApplicant>()
                                                .Include(x => x.DocumentReqApplicantDetails)
                                                    .ThenInclude(x => x.DocumentReqType)
                                                .Include(x => x.DocumentReqStatusTrackingHistories)
                                                .Where(x => x.Id == param.IdDocumentReqApplicant)
                                                .FirstOrDefaultAsync(CancellationToken);

                if (getDocumentReqApplicant == null)
                    throw new BadRequestException("Document request is not found");

                #region check authorization
                var checkAdminAccess = await _checkAdminAccessByIdBinusianHandler.CheckAdminAccessByIdBinusian(new CheckAdminAccessByIdBinusianRequest
                {
                    IdSchool = getDocumentReqApplicant.IdSchool,
                    IdBinusian = AuthInfo.UserId
                });

                if (!checkAdminAccess.HasAdminAccess)
                    throw new BadRequestException("You are not authorized to perform this action");
                #endregion

                if (getDocumentReqApplicant.IdDocumentReqStatusWorkflow != DocumentRequestStatusWorkflow.WaitingForApproval)
                    throw new BadRequestException("Failed to update status workflow of document request. Please contact administrator");

                // set approval
                getDocumentReqApplicant.ApprovalStatus = param.ApprovalStatus;
                getDocumentReqApplicant.ApprovalRemarks = string.IsNullOrEmpty(param.Remarks) ? null : param.Remarks;
                getDocumentReqApplicant.IdBinusianApprover = AuthInfo.UserId;
                getDocumentReqApplicant.ApprovalDate = _dateTime.ServerTime;
                _dbContext.Entity<MsDocumentReqApplicant>().Update(getDocumentReqApplicant);

                var totalPriceDocument = getDocumentReqApplicant.DocumentReqApplicantDetails
                                            .Sum(x => x.PriceInvoice);

                // Case when status is Waiting for Payment. Update start date and end date payment
                if(totalPriceDocument > 0)
                {
                    var maxInvoiceDueHoursPayment = getDocumentReqApplicant.DocumentReqApplicantDetails
                                                .Select(x => x.DocumentReqType)
                                                .Max(x => x.InvoiceDueHoursPayment);

                    var getTrDocumentReqPaymentMapping = await _dbContext.Entity<TrDocumentReqPaymentMapping>()
                                                            .Where(x => x.IdDocumentReqApplicant == param.IdDocumentReqApplicant)
                                                            .FirstOrDefaultAsync(CancellationToken);

                    if (getTrDocumentReqPaymentMapping == null)
                        throw new BadRequestException("Document request payment mapping is not found. Please contact administrator");

                    getTrDocumentReqPaymentMapping.StartDatePayment = _dateTime.ServerTime;
                    getTrDocumentReqPaymentMapping.EndDatePayment = _dateTime.ServerTime.AddHours(maxInvoiceDueHoursPayment == null ? 0 : Convert.ToDouble(maxInvoiceDueHoursPayment.Value));
                    _dbContext.Entity<TrDocumentReqPaymentMapping>().Update(getTrDocumentReqPaymentMapping);
                }

                if(param.ApprovalStatus == DocumentRequestApprovalStatus.Approved)
                {
                    // get latest status workflow
                    var getLatestStatusWorkflow = getDocumentReqApplicant.DocumentReqStatusTrackingHistories
                                                    .OrderByDescending(x => x.StatusDate)
                                                    .FirstOrDefault();

                    getLatestStatusWorkflow.Remarks = string.Format("{0} ({1}). Notes: {2}", param.ApprovalStatus.GetDescription(), getDocumentReqApplicant.ApprovalDate.Value.ToString("dd MMM yyyy, hh:mm tt"), (string.IsNullOrEmpty(param.Remarks) ? "-" : param.Remarks));
                    _dbContext.Entity<TrDocumentReqStatusTrackingHistory>().Update(getLatestStatusWorkflow);
                }

                await _dbContext.SaveChangesAsync(CancellationToken);
                await _transaction.CommitAsync(CancellationToken);

                var updateStatusWorkflow = await _addDocumentRequestWorkflowHandler.AddDocumentRequestWorkflow(new AddDocumentRequestWorkflowRequest
                {
                    IdDocumentReqApplicant = param.IdDocumentReqApplicant,
                    IdDocumentReqStatusWorkflow = param.ApprovalStatus == DocumentRequestApprovalStatus.Declined ? DocumentRequestStatusWorkflow.Declined : totalPriceDocument > 0 ? DocumentRequestStatusWorkflow.WaitingForPayment : DocumentRequestStatusWorkflow.OnProcess,
                    IdBinusianStaff = AuthInfo.UserId,
                    Remarks = param.ApprovalStatus == DocumentRequestApprovalStatus.Approved ? null : param.Remarks
                });

                if (!updateStatusWorkflow.IsSuccess)
                    throw new BadRequestException("Internal error. Please contact administrator");

                return Request.CreateApiResult2();
            }
            catch (Exception ex)
            {
                _transaction?.Rollback();
                throw new Exception(ex.Message);
            }
            finally
            {
                _transaction?.Dispose();
            }
        }
    }
}

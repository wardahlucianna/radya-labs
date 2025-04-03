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
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestApprover;
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestPayment;
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestWorkflow;
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.MasterDocumentRequest;
using BinusSchool.Document.FnDocument.DocumentRequest.DocumentRequestApprover;
using BinusSchool.Document.FnDocument.DocumentRequest.DocumentRequestPayment;
using BinusSchool.Document.FnDocument.DocumentRequest.DocumentRequestWorkflow;
using BinusSchool.Document.FnDocument.DocumentRequest.MasterDocumentRequest.Validator;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.DocumentDb.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.Document.FnDocument.DocumentRequest.MasterDocumentRequest
{
    public class SaveFinishAndCollectReqDocumentHandler : FunctionsHttpSingleHandler
    {
        private IDbContextTransaction _transaction;
        private readonly IDocumentDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        private readonly AddDocumentRequestWorkflowHandler _addDocumentRequestWorkflowHandler;
        private readonly GetDocumentRequestPaymentInfoHandler _getDocumentRequestPaymentInfoHandler;
        private readonly CheckAdminAccessByIdBinusianHandler _checkAdminAccessByIdBinusianHandler;

        public SaveFinishAndCollectReqDocumentHandler(
            IDocumentDbContext dbContext,
            IMachineDateTime dateTime,
            AddDocumentRequestWorkflowHandler addDocumentRequestWorkflowHandler,
            GetDocumentRequestPaymentInfoHandler getDocumentRequestPaymentInfoHandler,
            CheckAdminAccessByIdBinusianHandler checkAdminAccessByIdBinusianHandler)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
            _addDocumentRequestWorkflowHandler = addDocumentRequestWorkflowHandler;
            _getDocumentRequestPaymentInfoHandler = getDocumentRequestPaymentInfoHandler;
            _checkAdminAccessByIdBinusianHandler = checkAdminAccessByIdBinusianHandler;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<SaveFinishAndCollectReqDocumentRequest, SaveFinishAndCollectReqDocumentValidator>();

            try
            {
                _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

                var getDocumentReqApplicant = await _dbContext.Entity<MsDocumentReqApplicant>()
                                            .Include(x => x.DocumentReqApplicantDetails)
                                            .Include(x => x.DocumentReqApplicantCollections)
                                            .Where(x => x.Id == param.IdDocumentReqApplicant)
                                            .FirstOrDefaultAsync(CancellationToken);

                if (getDocumentReqApplicant == null)
                    throw new BadRequestException("Document request applicant is not found");

                #region check authorization
                var checkAdminAccess = await _checkAdminAccessByIdBinusianHandler.CheckAdminAccessByIdBinusian(new CheckAdminAccessByIdBinusianRequest
                {
                    IdSchool = getDocumentReqApplicant.IdSchool,
                    IdBinusian = AuthInfo.UserId
                });

                if (!checkAdminAccess.HasAdminAccess)
                    throw new BadRequestException("You are not authorized to perform this action");
                #endregion

                var documentPaymentInfoRaw = await _getDocumentRequestPaymentInfoHandler.GetDocumentRequestPaymentInfo(new GetDocumentRequestPaymentInfoRequest
                {
                    IdDocumentReqApplicantList = new List<string> { param.IdDocumentReqApplicant }
                });

                var documentPaymentInfo = documentPaymentInfoRaw.FirstOrDefault();

                var softCopyOnlyDocument = getDocumentReqApplicant.DocumentReqApplicantDetails
                                            .All(x => x.NeedSoftCopy && !x.NeedHardCopy);

                if ((getDocumentReqApplicant.IdDocumentReqStatusWorkflow != DocumentRequestStatusWorkflow.OnProcess ||
                    getDocumentReqApplicant.IdDocumentReqStatusWorkflow != DocumentRequestStatusWorkflow.Finished) &&
                    getDocumentReqApplicant.DocumentReqApplicantDetails.Where(x => string.IsNullOrEmpty(x.IdBinusianReceiver)).Any() &&
                    (documentPaymentInfo.PaymentStatus != PaymentStatus.Free && documentPaymentInfo.PaymentStatus != PaymentStatus.Paid)
                    )
                    throw new BadRequestException("Cannot finish document request. One or more validations are not met");

                var existingDocumentReqApplicantCollection = getDocumentReqApplicant.DocumentReqApplicantCollections
                                                                .FirstOrDefault();

                bool hasCollectedDocument = false;

                if (existingDocumentReqApplicantCollection == null)
                {
                    // create new MsDocumentReqApplicantCollection
                    var newDocumentReqApplicantCollection = new MsDocumentReqApplicantCollection
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdDocumentReqApplicant = param.IdDocumentReqApplicant,
                        FinishDate = _dateTime.ServerTime,
                        ScheduleCollectionDateStart = param.ScheduleCollectionDateStart,
                        ScheduleCollectionDateEnd = param.ScheduleCollectionDateEnd,
                        Remarks = param.Remarks,
                        IdVenue = param.IdVenue,
                        CollectedBy = softCopyOnlyDocument ? "(Softcopy Only Document(s))" : param.CollectedBy,
                        CollectedDate = softCopyOnlyDocument ? _dateTime.ServerTime : param.CollectedDate
                    };

                    _dbContext.Entity<MsDocumentReqApplicantCollection>().Add(newDocumentReqApplicantCollection);

                    if (newDocumentReqApplicantCollection.CollectedDate != null)
                        hasCollectedDocument = true;
                }
                else
                {
                    // update MsDocumentReqApplicantCollection
                    existingDocumentReqApplicantCollection.ScheduleCollectionDateStart = param.ScheduleCollectionDateStart;
                    existingDocumentReqApplicantCollection.ScheduleCollectionDateEnd = param.ScheduleCollectionDateEnd;
                    existingDocumentReqApplicantCollection.Remarks = param.Remarks;
                    existingDocumentReqApplicantCollection.IdVenue = param.IdVenue;
                    existingDocumentReqApplicantCollection.CollectedBy = softCopyOnlyDocument ? "(Softcopy Only Document(s))" : param.CollectedBy;
                    existingDocumentReqApplicantCollection.CollectedDate = softCopyOnlyDocument ? _dateTime.ServerTime : param.CollectedDate;

                    _dbContext.Entity<MsDocumentReqApplicantCollection>().Update(existingDocumentReqApplicantCollection);

                    if (existingDocumentReqApplicantCollection.CollectedDate != null)
                        hasCollectedDocument = true;
                }

                await _dbContext.SaveChangesAsync(CancellationToken);
                await _transaction.CommitAsync(CancellationToken);

                var newDocumentRequestStatusWorkflow = new DocumentRequestStatusWorkflow();

                if (hasCollectedDocument)
                    newDocumentRequestStatusWorkflow = DocumentRequestStatusWorkflow.Collected;
                else
                    newDocumentRequestStatusWorkflow = DocumentRequestStatusWorkflow.Finished;

                var addDocumentRequestWorkflow = await _addDocumentRequestWorkflowHandler.AddDocumentRequestWorkflow(new AddDocumentRequestWorkflowRequest
                {
                    IdDocumentReqApplicant = param.IdDocumentReqApplicant,
                    IdDocumentReqStatusWorkflow = newDocumentRequestStatusWorkflow,
                    IdBinusianStaff = AuthInfo.UserId,
                    Remarks = param.Remarks
                });

                if (!addDocumentRequestWorkflow.IsSuccess)
                    throw new BadRequestException("Internal server error, please contact administrator");

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

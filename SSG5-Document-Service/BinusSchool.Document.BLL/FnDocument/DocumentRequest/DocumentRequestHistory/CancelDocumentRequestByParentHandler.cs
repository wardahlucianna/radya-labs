using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestHistory;
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestWorkflow;
using BinusSchool.Document.FnDocument.DocumentRequest.DocumentRequestHistory.Validator;
using BinusSchool.Document.FnDocument.DocumentRequest.DocumentRequestWorkflow;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.DocumentDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Document.FnDocument.DocumentRequest.DocumentRequestHistory
{
    public class CancelDocumentRequestByParentHandler : FunctionsHttpSingleHandler
    {
        private readonly IDocumentDbContext _dbContext;
        private readonly AddDocumentRequestWorkflowHandler _addDocumentRequestWorkflowHandler;

        public CancelDocumentRequestByParentHandler(
            IDocumentDbContext dbContext,
            AddDocumentRequestWorkflowHandler addDocumentRequestWorkflowHandler)
        {
            _dbContext = dbContext;
            _addDocumentRequestWorkflowHandler = addDocumentRequestWorkflowHandler;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<CancelDocumentRequestByParentRequest, CancelDocumentRequestByParentValidator>();

            var getDocumentReqApplicantAndLatestStatus = await _dbContext.Entity<MsDocumentReqApplicant>()
                                                    .Include(x => x.DocumentReqStatusTrackingHistories)
                                                        .ThenInclude(x => x.DocumentReqStatusWorkflow)
                                                    .Where(x => x.Id == param.IdDocumentReqApplicant)
                                                    .Select(x => new
                                                    {
                                                        DocumentReqApplicant = x,
                                                        LatestDocumentReqStatusWorkflow = x.DocumentReqStatusTrackingHistories
                                                                                            .OrderByDescending(y => y.StatusDate)
                                                                                            .FirstOrDefault()
                                                    })
                                                    .FirstOrDefaultAsync(CancellationToken);

            if (getDocumentReqApplicantAndLatestStatus == null)
                throw new BadRequestException("Document request applicant is not found");

            if (
                (getDocumentReqApplicantAndLatestStatus.LatestDocumentReqStatusWorkflow.IdDocumentReqStatusWorkflow != DocumentRequestStatusWorkflow.WaitingForApproval &&
                getDocumentReqApplicantAndLatestStatus.LatestDocumentReqStatusWorkflow.IdDocumentReqStatusWorkflow != DocumentRequestStatusWorkflow.WaitingForPayment &&
                getDocumentReqApplicantAndLatestStatus.LatestDocumentReqStatusWorkflow.IdDocumentReqStatusWorkflow != DocumentRequestStatusWorkflow.WaitingForPaymentVerification &&
                getDocumentReqApplicantAndLatestStatus.LatestDocumentReqStatusWorkflow.IdDocumentReqStatusWorkflow != DocumentRequestStatusWorkflow.Finished &&
                getDocumentReqApplicantAndLatestStatus.LatestDocumentReqStatusWorkflow.IdDocumentReqStatusWorkflow != DocumentRequestStatusWorkflow.Collected &&
                getDocumentReqApplicantAndLatestStatus.LatestDocumentReqStatusWorkflow.IdDocumentReqStatusWorkflow != DocumentRequestStatusWorkflow.Canceled &&
                getDocumentReqApplicantAndLatestStatus.LatestDocumentReqStatusWorkflow.IdDocumentReqStatusWorkflow != DocumentRequestStatusWorkflow.Declined) ||
                getDocumentReqApplicantAndLatestStatus.LatestDocumentReqStatusWorkflow.IsOnProcess
                )
                throw new BadRequestException($"Cannot delete document request with status: {getDocumentReqApplicantAndLatestStatus.LatestDocumentReqStatusWorkflow.DocumentReqStatusWorkflow.ParentDescription + (getDocumentReqApplicantAndLatestStatus.LatestDocumentReqStatusWorkflow.IsOnProcess ? " (On Process)" : "")}");

            // change status to canceled
            var changeStatusToCancel = await _addDocumentRequestWorkflowHandler.AddDocumentRequestWorkflow(new AddDocumentRequestWorkflowRequest
            {
                IdDocumentReqApplicant = param.IdDocumentReqApplicant,
                IdDocumentReqStatusWorkflow = DocumentRequestStatusWorkflow.Canceled
            });

            if (changeStatusToCancel.IsSuccess == false)
                throw new BadRequestException(changeStatusToCancel.ErrorMessage);

            return Request.CreateApiResult2();
        }
    }
}

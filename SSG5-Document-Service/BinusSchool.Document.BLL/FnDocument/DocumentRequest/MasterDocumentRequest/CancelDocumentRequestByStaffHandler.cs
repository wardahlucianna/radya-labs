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

namespace BinusSchool.Document.FnDocument.DocumentRequest.MasterDocumentRequest
{
    public class CancelDocumentRequestByStaffHandler : FunctionsHttpSingleHandler
    {
        private readonly IDocumentDbContext _dbContext;
        private readonly AddDocumentRequestWorkflowHandler _addDocumentRequestWorkflowHandler;
        private readonly GetDocumentRequestPaymentInfoHandler _getDocumentRequestPaymentInfoHandler;
        private readonly CheckAdminAccessByIdBinusianHandler _checkAdminAccessByIdBinusianHandler;

        public CancelDocumentRequestByStaffHandler(
            IDocumentDbContext dbContext,
            AddDocumentRequestWorkflowHandler addDocumentRequestWorkflowHandler,
            GetDocumentRequestPaymentInfoHandler getDocumentRequestPaymentInfoHandler,
            CheckAdminAccessByIdBinusianHandler checkAdminAccessByIdBinusianHandler)
        {
            _dbContext = dbContext;
            _addDocumentRequestWorkflowHandler = addDocumentRequestWorkflowHandler;
            _getDocumentRequestPaymentInfoHandler = getDocumentRequestPaymentInfoHandler;
            _checkAdminAccessByIdBinusianHandler = checkAdminAccessByIdBinusianHandler;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<CancelDocumentRequestByStaffRequest, CancelDocumentRequestByStaffValidator>();

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

            #region check authorization
            var checkAdminAccess = await _checkAdminAccessByIdBinusianHandler.CheckAdminAccessByIdBinusian(new CheckAdminAccessByIdBinusianRequest
            {
                IdSchool = getDocumentReqApplicantAndLatestStatus.DocumentReqApplicant.IdSchool,
                IdBinusian = AuthInfo.UserId
            });

            if (!checkAdminAccess.HasAdminAccess)
                throw new BadRequestException("You are not authorized to perform this action");
            #endregion

            var getPaymentInfoRawList = await _getDocumentRequestPaymentInfoHandler.GetDocumentRequestPaymentInfo(new GetDocumentRequestPaymentInfoRequest
            {
                IdDocumentReqApplicantList = new List<string> { param.IdDocumentReqApplicant }
            });

            var getPaymentInfo = getPaymentInfoRawList.FirstOrDefault();

            if (
                (getDocumentReqApplicantAndLatestStatus.LatestDocumentReqStatusWorkflow.IdDocumentReqStatusWorkflow != DocumentRequestStatusWorkflow.Finished &&
                getDocumentReqApplicantAndLatestStatus.LatestDocumentReqStatusWorkflow.IdDocumentReqStatusWorkflow != DocumentRequestStatusWorkflow.Collected &&
                getDocumentReqApplicantAndLatestStatus.LatestDocumentReqStatusWorkflow.IdDocumentReqStatusWorkflow != DocumentRequestStatusWorkflow.Canceled &&
                getDocumentReqApplicantAndLatestStatus.LatestDocumentReqStatusWorkflow.IdDocumentReqStatusWorkflow != DocumentRequestStatusWorkflow.Declined) &&
                    (getPaymentInfo == null ? true : getPaymentInfo.PaymentStatus == PaymentStatus.Expired ? true : false)
                )
                throw new BadRequestException($"Cannot cancel document request with status: {getDocumentReqApplicantAndLatestStatus.LatestDocumentReqStatusWorkflow.DocumentReqStatusWorkflow.StaffDescription}");

            // change status to canceled
            var changeStatusToCancel = await _addDocumentRequestWorkflowHandler.AddDocumentRequestWorkflow(new AddDocumentRequestWorkflowRequest
            {
                IdDocumentReqApplicant = param.IdDocumentReqApplicant,
                IdDocumentReqStatusWorkflow = DocumentRequestStatusWorkflow.Canceled,
                Remarks = param.Remarks,
                IdBinusianStaff = AuthInfo.UserId
            });

            if (changeStatusToCancel.IsSuccess == false)
                throw new BadRequestException(changeStatusToCancel.ErrorMessage);

            return Request.CreateApiResult2();
        }
    }
}

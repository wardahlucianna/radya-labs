using System;
using System.Collections.Generic;
using System.IO;
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
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.Helper;
using BinusSchool.Document.FnDocument.DocumentRequest.DocumentRequestApprover;
using BinusSchool.Document.FnDocument.DocumentRequest.DocumentRequestPayment.Validator;
using BinusSchool.Document.FnDocument.DocumentRequest.DocumentRequestWorkflow;
using BinusSchool.Document.FnDocument.DocumentRequest.Helper;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.DocumentDb.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;

namespace BinusSchool.Document.FnDocument.DocumentRequest.DocumentRequestPayment
{
    public class SavePaymentApprovalHandler : FunctionsHttpSingleHandler
    {
        private static readonly IDictionary<string, string[]> _allowedExtensions = new Dictionary<string, string[]>()
        {
            { "image", new[]{ ".png", ".jpg", ".jpeg" } },
            { "document", new[]{ ".pdf" } }
        };

        private IDbContextTransaction _transaction;
        private readonly IDocumentDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly IMachineDateTime _dateTime;
        private readonly GetDocumentRequestPaymentInfoHandler _getDocumentRequestPaymentInfoHandler;
        private readonly AddDocumentRequestWorkflowHandler _addDocumentRequestWorkflowHandler;
        private readonly CheckAdminAccessByIdBinusianHandler _checkAdminAccessByIdBinusianHandler;

        public SavePaymentApprovalHandler(
            IDocumentDbContext dbContext,
            IConfiguration configuration,
            IMachineDateTime dateTime,
            GetDocumentRequestPaymentInfoHandler getDocumentRequestPaymentInfoHandler,
            AddDocumentRequestWorkflowHandler addDocumentRequestWorkflowHandler,
            CheckAdminAccessByIdBinusianHandler checkAdminAccessByIdBinusianHandler)
        {
            _dbContext = dbContext;
            _configuration = configuration;
            _dateTime = dateTime;
            _getDocumentRequestPaymentInfoHandler = getDocumentRequestPaymentInfoHandler;
            _addDocumentRequestWorkflowHandler = addDocumentRequestWorkflowHandler;
            _checkAdminAccessByIdBinusianHandler = checkAdminAccessByIdBinusianHandler;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBodyForm<SavePaymentApprovalRequest, SavePaymentApprovalValidator>();

            try
            {
                _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

                var verificationDate = _dateTime.ServerTime;

                var getDocumentRequestApplicant = await _dbContext.Entity<MsDocumentReqApplicant>()
                                                    .Include(x => x.DocumentReqPaymentMappings)
                                                    .Include(x => x.DocumentReqStatusTrackingHistories)
                                                    .Where(x => x.Id == param.IdDocumentReqApplicant)
                                                    .FirstOrDefaultAsync(CancellationToken);

                if (getDocumentRequestApplicant == null)
                    throw new BadRequestException("Document request applicant is not found");

                #region check authorization
                var checkAdminAccess = await _checkAdminAccessByIdBinusianHandler.CheckAdminAccessByIdBinusian(new CheckAdminAccessByIdBinusianRequest
                {
                    IdSchool = getDocumentRequestApplicant.IdSchool,
                    IdBinusian = AuthInfo.UserId
                });

                if (!checkAdminAccess.HasAdminAccess)
                    throw new BadRequestException("You are not authorized to perform this action");
                #endregion

                var getDocumentRequestPaymentMappping = getDocumentRequestApplicant.DocumentReqPaymentMappings.FirstOrDefault();

                if (getDocumentRequestPaymentMappping == null)
                    throw new BadRequestException("Document request payment mapping is not found");

                var getDocumentReqPaymentManual = await _dbContext.Entity<TrDocumentReqPaymentManual>()
                                                    .Where(x => x.Id == getDocumentRequestPaymentMappping.IdDocumentReqPaymentManual)
                                                    .FirstOrDefaultAsync(CancellationToken);

                var getPaymentInfoRaw = await _getDocumentRequestPaymentInfoHandler.GetDocumentRequestPaymentInfo(new GetDocumentRequestPaymentInfoRequest
                {
                    IdDocumentReqApplicantList = new List<string>() { param.IdDocumentReqApplicant }
                });

                var getPaymentInfo = getPaymentInfoRaw.FirstOrDefault();

                if(getPaymentInfo.PaymentStatus != PaymentStatus.Unpaid || getDocumentRequestApplicant.IdDocumentReqStatusWorkflow == DocumentRequestStatusWorkflow.Canceled)
                    throw new BadRequestException("Cannot verify payment of the document request. Please contact administrator");

                // upload transfer evidance (if staff upload new evidance)
                #region Upload File
                var uploadFileHelper = new SoftCopyDocumentBlobHelper(_dateTime, _configuration);

                string fileName = "";
                var uploadedFileResult = new UploadFileHelperResult();

                var file = Request.Form.Files.FirstOrDefault();
                FileInfo fileInfo = null;

                if (file != null && file.Length != 0)
                {
                    fileInfo = new FileInfo(file.FileName);
                    if (!_allowedExtensions["image"].Any(x => x == fileInfo.Extension) && !_allowedExtensions["document"].Any(x => x == fileInfo.Extension))
                        throw new BadRequestException($"File extenstion is not allowed. Allowed file: {string.Join(", ", _allowedExtensions["image"]) + ", " + string.Join(", ", _allowedExtensions["document"])}");

                    uploadedFileResult = await uploadFileHelper.UploadFileAsync(getDocumentRequestApplicant.IdStudent, file);
                }
                #endregion

                // update MsDocumentRequestApplicant
                getDocumentRequestPaymentMappping.IdDocumentReqPaymentMethod = param.IdDocumentReqPaymentMethod;

                // parent already confirm payment
                if (getDocumentReqPaymentManual != null)
                {
                    #region Remove Existing File
                    await uploadFileHelper.RemoveFileIfExists(getDocumentRequestApplicant.IdStudent, getDocumentReqPaymentManual.AttachmentFileName);
                    #endregion

                    getDocumentReqPaymentManual.PaymentDate = param.PaymentDate;
                    getDocumentReqPaymentManual.PaidAmount = param.PaidAmount;
                    getDocumentReqPaymentManual.SenderAccountName = param.SenderAccountName;
                    getDocumentReqPaymentManual.AttachmentFileName = uploadedFileResult != null && !string.IsNullOrEmpty(uploadedFileResult.UploadedFileName) ? uploadedFileResult.UploadedFileName : getDocumentReqPaymentManual.AttachmentFileName;
                    getDocumentReqPaymentManual.AttachmentImageUrl = uploadedFileResult != null && !string.IsNullOrEmpty(uploadedFileResult.UploadedFileName) ? uploadedFileResult.UploadedFileUrl : getDocumentReqPaymentManual.AttachmentImageUrl;
                    getDocumentReqPaymentManual.VerificationDate = verificationDate;
                    getDocumentReqPaymentManual.PaymentStatus = param.VerificationStatus ? 1 : 0;
                    getDocumentReqPaymentManual.VerificationStatus = param.VerificationStatus ? 1 : 0;
                    getDocumentReqPaymentManual.Remarks = param.Remarks;
                    getDocumentReqPaymentManual.IdBinusianVerificator = AuthInfo.UserId;

                    _dbContext.Entity<TrDocumentReqPaymentManual>().Update(getDocumentReqPaymentManual);
                }
                else
                {
                    var newTrDocumentReqPaymentManual = new TrDocumentReqPaymentManual
                    {
                        Id = Guid.NewGuid().ToString(),
                        PaidAmount = param.PaidAmount,
                        PaymentDate = param.PaymentDate,
                        AttachmentFileName = uploadedFileResult != null && !string.IsNullOrEmpty(uploadedFileResult.UploadedFileName) ? uploadedFileResult.UploadedFileName : null,
                        AttachmentImageUrl = uploadedFileResult != null && !string.IsNullOrEmpty(uploadedFileResult.UploadedFileName) ? uploadedFileResult.UploadedFileUrl : null,
                        SenderAccountName = param.SenderAccountName,
                        PaymentStatus = param.VerificationStatus ? 1 : 0,
                        VerificationStatus = param.VerificationStatus ? 1 : 0,
                        VerificationDate = verificationDate,
                        Remarks = param.Remarks,
                        IdBinusianVerificator = AuthInfo.UserId
                    };

                    _dbContext.Entity<TrDocumentReqPaymentManual>().Add(newTrDocumentReqPaymentManual);

                    getDocumentRequestPaymentMappping.IdDocumentReqPaymentManual = newTrDocumentReqPaymentManual.Id;
                }

                _dbContext.Entity<TrDocumentReqPaymentMapping>().Update(getDocumentRequestPaymentMappping);

                // get latest status workflow
                var getLatestStatusWorkflow = getDocumentRequestApplicant.DocumentReqStatusTrackingHistories
                                                .OrderByDescending(x => x.StatusDate)
                                                .FirstOrDefault();

                getLatestStatusWorkflow.Remarks = string.Format("{0} ({1}). Notes: {2}", (param.VerificationStatus ? "Approved" : "Declined"), verificationDate.ToString("dd MMM yyyy, hh:mm tt"), (string.IsNullOrEmpty(param.Remarks) ? "-" : param.Remarks));
                _dbContext.Entity<TrDocumentReqStatusTrackingHistory>().Update(getLatestStatusWorkflow);

                await _dbContext.SaveChangesAsync(CancellationToken);
                await _transaction.CommitAsync(CancellationToken);

                // Add TrDocumentReqStatusTrackingHistory
                var addTrDocumentReqStatusTrackingHistory = await _addDocumentRequestWorkflowHandler.AddDocumentRequestWorkflow(new AddDocumentRequestWorkflowRequest
                {
                    IdDocumentReqApplicant = getDocumentRequestApplicant.Id,
                    IdDocumentReqStatusWorkflow = param.VerificationStatus ? DocumentRequestStatusWorkflow.OnProcess : DocumentRequestStatusWorkflow.WaitingForPayment,
                    IdBinusianStaff = AuthInfo.UserId
                });

                if (!addTrDocumentReqStatusTrackingHistory.IsSuccess)
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

            throw new NotImplementedException();
        }
    }
}

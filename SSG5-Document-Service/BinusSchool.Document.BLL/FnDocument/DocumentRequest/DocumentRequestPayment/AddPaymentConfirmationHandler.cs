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
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestPayment;
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestWorkflow;
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.Helper;
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
    public class AddPaymentConfirmationHandler : FunctionsHttpSingleHandler
    {
        private static readonly IDictionary<string, string[]> _allowedExtensions = new Dictionary<string, string[]>()
        {
            { "image", new[]{".png", ".jpg", ".jpeg"} },
            { "document", new[]{".pdf" } }
        };

        private IDbContextTransaction _transaction;
        private readonly IDocumentDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        private readonly IConfiguration _configuration;
        private readonly AddDocumentRequestWorkflowHandler _addDocumentRequestWorkflowHandler;

        public AddPaymentConfirmationHandler(
            IDocumentDbContext dbContext,
            IMachineDateTime dateTime,
            IConfiguration configuration,
            AddDocumentRequestWorkflowHandler addDocumentRequestWorkflowHandler)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
            _configuration = configuration;
            _addDocumentRequestWorkflowHandler = addDocumentRequestWorkflowHandler;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBodyForm<AddPaymentConfirmationRequest, AddPaymentConfirmationValidator>();

            try
            {
                _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

                var getDocumentReqPaymentMapping = await _dbContext.Entity<TrDocumentReqPaymentMapping>()
                                                    .Include(x => x.DocumentReqApplicant)
                                                    .Where(x => x.IdDocumentReqApplicant == param.IdDocumentReqApplicant)
                                                    .FirstOrDefaultAsync(CancellationToken);

                if (getDocumentReqPaymentMapping == null)
                    throw new BadRequestException("Document request payment mapping is not found");

                #region Upload File
                var uploadFileHelper = new PaymentEvidanceDocumentBlobHelper(_dateTime, _configuration);

                string fileName = "";
                var uploadedFileResult = new UploadFileHelperResult();

                var file = Request.Form.Files.FirstOrDefault();

                if (file is null || file.Length == 0)
                    throw new BadRequestException("Document file not found");

                if (file != null && file.Length != 0)
                {
                    var fileInfo = new FileInfo(file.FileName);
                    if (!_allowedExtensions["image"].Any(x => x == fileInfo.Extension) && !_allowedExtensions["document"].Any(x => x == fileInfo.Extension))
                        throw new BadRequestException($"File extenstion is not allowed. Allowed file: {string.Join(", ", _allowedExtensions["image"]) + ", " + string.Join(", ", _allowedExtensions["document"])}");

                    uploadedFileResult = await uploadFileHelper.UploadFileAsync(getDocumentReqPaymentMapping.DocumentReqApplicant.IdStudent, file);
                }
                #endregion

                // add TrDocumentReqPaymentManual
                var newTrDocumentReqPaymentManual = new TrDocumentReqPaymentManual
                {
                    Id = Guid.NewGuid().ToString(),
                    PaidAmount = param.PaidAmount,
                    PaymentStatus = 1,
                    PaymentDate = _dateTime.ServerTime,
                    SenderAccountName = param.SenderAccountName,
                    AttachmentFileName = uploadedFileResult.UploadedFileName,
                    AttachmentImageUrl = uploadedFileResult.UploadedFileUrl
                };
                _dbContext.Entity<TrDocumentReqPaymentManual>().Add(newTrDocumentReqPaymentManual);

                // update TrDocumentReqPaymentMapping
                getDocumentReqPaymentMapping.IdDocumentReqPaymentManual = newTrDocumentReqPaymentManual.Id;
                getDocumentReqPaymentMapping.IdDocumentReqPaymentMethod = param.IdDocumentReqPaymentMethod;
                _dbContext.Entity<TrDocumentReqPaymentMapping>().Update(getDocumentReqPaymentMapping);

                await _dbContext.SaveChangesAsync(CancellationToken);
                await _transaction.CommitAsync(CancellationToken);

                // Add TrDocumentReqStatusTrackingHistory
                var addTrDocumentReqStatusTrackingHistory = await _addDocumentRequestWorkflowHandler.AddDocumentRequestWorkflow(new AddDocumentRequestWorkflowRequest
                {
                    IdDocumentReqApplicant = param.IdDocumentReqApplicant,
                    IdDocumentReqStatusWorkflow = DocumentRequestStatusWorkflow.WaitingForPaymentVerification
                });

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

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
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestApprover;
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.Helper;
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.MasterDocumentRequest;
using BinusSchool.Document.FnDocument.DocumentRequest.DocumentRequestApprover;
using BinusSchool.Document.FnDocument.DocumentRequest.Helper;
using BinusSchool.Document.FnDocument.DocumentRequest.MasterDocumentRequest.Validator;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.DocumentDb.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;

namespace BinusSchool.Document.FnDocument.DocumentRequest.MasterDocumentRequest
{
    public class SaveSoftCopyDocumentHandler : FunctionsHttpSingleHandler
    {
        private static readonly IDictionary<string, string[]> _allowedExtensions = new Dictionary<string, string[]>()
        {
            { "image", new[]{".png", ".jpg", ".jpeg"} },
            { "document", new[]{".pdf", ".doc", ".docx" } }
        };

        private IDbContextTransaction _transaction;
        private readonly IDocumentDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly IMachineDateTime _dateTime;
        private readonly CheckAdminAccessByIdBinusianHandler _checkAdminAccessByIdBinusianHandler;

        public SaveSoftCopyDocumentHandler(
            IDocumentDbContext dbContext,
            IConfiguration configuration,
            IMachineDateTime dateTime,
            CheckAdminAccessByIdBinusianHandler checkAdminAccessByIdBinusianHandler)
        {
            _dbContext = dbContext;
            _configuration = configuration;
            _dateTime = dateTime;
            _checkAdminAccessByIdBinusianHandler = checkAdminAccessByIdBinusianHandler;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBodyForm<SaveSoftCopyDocumentRequest, SaveSoftCopyDocumentValidator>();

            try
            {
                _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

                var getDocumentReqApplicantDetail = await _dbContext.Entity<TrDocumentReqApplicantDetail>()
                                                        .Include(x => x.DocumentReqAttachments)
                                                        .Include(x => x.DocumentReqApplicant)
                                                        .Where(x => x.Id == param.IdDocumentReqApplicantDetail)
                                                        .FirstOrDefaultAsync(CancellationToken);

                if(getDocumentReqApplicantDetail == null)
                    throw new BadRequestException("Document request applicant detail is not found");


                var getDocumentReqApplicant = getDocumentReqApplicantDetail.DocumentReqApplicant;

                #region check authorization
                var checkAdminAccess = await _checkAdminAccessByIdBinusianHandler.CheckAdminAccessByIdBinusian(new CheckAdminAccessByIdBinusianRequest
                {
                    IdSchool = getDocumentReqApplicant.IdSchool,
                    IdBinusian = AuthInfo.UserId
                });

                if (!checkAdminAccess.HasAdminAccess)
                    throw new BadRequestException("You are not authorized to perform this action");
                #endregion

                var getExistingDocumentReqAttachment = getDocumentReqApplicantDetail.DocumentReqAttachments
                                                        .FirstOrDefault();

                bool hasFile = false;

                #region Upload File
                var uploadFileHelper = new SoftCopyDocumentBlobHelper(_dateTime, _configuration);

                string fileName = "";
                var uploadedFileResult = new UploadFileHelperResult();

                var file = Request.Form.Files.FirstOrDefault();
                FileInfo fileInfo = null;

                //if (file is null || file.Length == 0)
                //    throw new BadRequestException("Document file not found");

                if (file != null && file.Length != 0)
                {
                    hasFile = true;

                    fileInfo = new FileInfo(file.FileName);
                    if (!_allowedExtensions["image"].Any(x => x == fileInfo.Extension) && !_allowedExtensions["document"].Any(x => x == fileInfo.Extension))
                        throw new BadRequestException($"File extenstion is not allowed. Allowed file: {string.Join(", ", _allowedExtensions["image"]) + ", " + string.Join(", ", _allowedExtensions["document"])}");

                    uploadedFileResult = await uploadFileHelper.UploadFileAsync(getDocumentReqApplicantDetail.DocumentReqApplicant.IdStudent, file);
                }
                #endregion

                if (hasFile)
                {
                    // remove existing file
                    if (getExistingDocumentReqAttachment != null && !string.IsNullOrEmpty(getExistingDocumentReqAttachment.FileName))
                    {
                        await uploadFileHelper.RemoveFileIfExists(getDocumentReqApplicantDetail.DocumentReqApplicant.IdStudent, getExistingDocumentReqAttachment.FileName);

                        // remove existing TrDocumentReqAttachment
                        _dbContext.Entity<TrDocumentReqAttachment>().Remove(getExistingDocumentReqAttachment);
                    }

                    // add new TrDocumentReqAttachment
                    var newTrDocumentReqAttachment = new TrDocumentReqAttachment
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdDocumentReqApplicantDetail = param.IdDocumentReqApplicantDetail,
                        OriginalFileName = file.FileName,
                        FileName = uploadedFileResult.UploadedFileName,
                        FileType = _allowedExtensions["image"].Any(x => x == fileInfo.Extension) ? "image" : "document",
                        FileExtension = fileInfo.Extension,
                        FileSize = file.Length,
                        FilePath = uploadedFileResult.UploadedFileUrl,
                        ShowToParent = getDocumentReqApplicantDetail.NeedSoftCopy ? true : param.ShowToParent,
                        IdUserModifier = AuthInfo.UserId
                    };

                    _dbContext.Entity<TrDocumentReqAttachment>().Add(newTrDocumentReqAttachment);
                }
                else
                {
                    // update existing TrDocumentReqAttachment
                    getExistingDocumentReqAttachment.ShowToParent = getDocumentReqApplicantDetail.NeedSoftCopy ? true : param.ShowToParent;
                    _dbContext.Entity<TrDocumentReqAttachment>().Update(getExistingDocumentReqAttachment);
                }

                // change DocumentIsReady = true, if softcopy only
                if (!getDocumentReqApplicantDetail.NeedHardCopy && getDocumentReqApplicantDetail.NeedSoftCopy)
                {
                    getDocumentReqApplicantDetail.ReceivedDateByStaff = _dateTime.ServerTime;
                    getDocumentReqApplicantDetail.IdBinusianReceiver = AuthInfo.UserId;
                    _dbContext.Entity<TrDocumentReqApplicantDetail>().Update(getDocumentReqApplicantDetail);
                }
                
                await _dbContext.SaveChangesAsync(CancellationToken);
                await _transaction.CommitAsync(CancellationToken);

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

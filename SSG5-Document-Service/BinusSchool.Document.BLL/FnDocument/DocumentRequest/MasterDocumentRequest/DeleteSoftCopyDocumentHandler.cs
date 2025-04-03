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
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestApprover;
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
    public class DeleteSoftCopyDocumentHandler : FunctionsHttpSingleHandler
    {

        private IDbContextTransaction _transaction;
        private readonly IDocumentDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly IMachineDateTime _dateTime;
        private readonly CheckAdminAccessByIdBinusianHandler _checkAdminAccessByIdBinusianHandler;

        public DeleteSoftCopyDocumentHandler(
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

        public DeleteSoftCopyDocumentHandler(IDocumentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<DeleteSoftCopyDocumentRequest, DeleteSoftCopyDocumentValidator>();

            try
            {
                _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

                var getDocumentReqApplicantDetail = await _dbContext.Entity<TrDocumentReqApplicantDetail>()
                                                        .Include(x => x.DocumentReqAttachments)
                                                        .Include(x => x.DocumentReqApplicant)
                                                        .Where(x => x.Id == param.IdDocumentReqApplicantDetail)
                                                        .FirstOrDefaultAsync(CancellationToken);

                if (getDocumentReqApplicantDetail == null)
                    throw new BadRequestException("Document request applicant detail is not found");

                #region check authorization
                var checkAdminAccess = await _checkAdminAccessByIdBinusianHandler.CheckAdminAccessByIdBinusian(new CheckAdminAccessByIdBinusianRequest
                {
                    IdSchool = getDocumentReqApplicantDetail.DocumentReqApplicant.IdSchool,
                    IdBinusian = AuthInfo.UserId
                });

                if (!checkAdminAccess.HasAdminAccess)
                    throw new BadRequestException("You are not authorized to perform this action");
                #endregion

                var getExistingDocumentReqAttachment = getDocumentReqApplicantDetail.DocumentReqAttachments
                                                        .FirstOrDefault();

                if (getExistingDocumentReqAttachment == null)
                    throw new BadRequestException("Document request soft copy is not found");

                var uploadFileHelper = new SoftCopyDocumentBlobHelper(_dateTime, _configuration);

                await uploadFileHelper.RemoveFileIfExists(getDocumentReqApplicantDetail.DocumentReqApplicant.IdStudent, getExistingDocumentReqAttachment.FileName);

                // delete soft copy file
                _dbContext.Entity<TrDocumentReqAttachment>().Remove(getExistingDocumentReqAttachment);

                // change DocumentIsReady = false
                getDocumentReqApplicantDetail.ReceivedDateByStaff = null;
                getDocumentReqApplicantDetail.IdBinusianReceiver = null;
                _dbContext.Entity<TrDocumentReqApplicantDetail>().Update(getDocumentReqApplicantDetail);

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

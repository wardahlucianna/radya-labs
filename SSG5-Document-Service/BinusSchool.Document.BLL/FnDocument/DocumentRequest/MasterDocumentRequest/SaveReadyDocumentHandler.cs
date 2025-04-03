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
using BinusSchool.Document.FnDocument.DocumentRequest.MasterDocumentRequest.Validator;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.DocumentDb.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.Document.FnDocument.DocumentRequest.MasterDocumentRequest
{
    public class SaveReadyDocumentHandler : FunctionsHttpSingleHandler
    {
        private readonly IDocumentDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        private readonly CheckAdminAccessByIdBinusianHandler _checkAdminAccessByIdBinusianHandler;
        private IDbContextTransaction _transaction;

        public SaveReadyDocumentHandler(
            IDocumentDbContext dbContext,
            IMachineDateTime dateTime,
            CheckAdminAccessByIdBinusianHandler checkAdminAccessByIdBinusianHandler)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
            _checkAdminAccessByIdBinusianHandler = checkAdminAccessByIdBinusianHandler;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<SaveReadyDocumentRequest, SaveReadyDocumentValidator>();

            try
            {
                _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

                var getDocumentReqApplicantDetailList = await _dbContext.Entity<TrDocumentReqApplicantDetail>()
                                                        .Include(x => x.DocumentReqAttachments)
                                                        .Include(x => x.DocumentReqType)
                                                        .Include(x => x.DocumentReqApplicant)
                                                        .Where(x => param.ChecklistStatusList.Select(y => y.IdDocumentReqApplicantDetail).Any(y => y == x.Id))
                                                        .ToListAsync(CancellationToken);

                var getDocumentReqApplicant = getDocumentReqApplicantDetailList
                                                .Select(x => x.DocumentReqApplicant)
                                                .FirstOrDefault();

                #region check authorization
                var checkAdminAccess = await _checkAdminAccessByIdBinusianHandler.CheckAdminAccessByIdBinusian(new CheckAdminAccessByIdBinusianRequest
                {
                    IdSchool = getDocumentReqApplicant.IdSchool,
                    IdBinusian = AuthInfo.UserId
                });

                if (!checkAdminAccess.HasAdminAccess)
                    throw new BadRequestException("You are not authorized to perform this action");
                #endregion

                foreach (var paramChecklistStatus in param.ChecklistStatusList)
                {
                    var getDocumentReqApplicantDetail = getDocumentReqApplicantDetailList
                                                        .Where(x => x.Id == paramChecklistStatus.IdDocumentReqApplicantDetail)
                                                        .FirstOrDefault();

                    if (getDocumentReqApplicantDetail == null)
                        throw new BadRequestException("Internal error, cannot find the document request applicant detail. Please contact administrator");

                    // validation for softcopy only
                    if (paramChecklistStatus.IsChecked)
                    {
                        if (getDocumentReqApplicantDetail.NeedSoftCopy)
                        {
                            // attachment should be attached before can check the "Document is Ready"
                            if(getDocumentReqApplicantDetail.DocumentReqAttachments == null || !getDocumentReqApplicantDetail.DocumentReqAttachments.Any()) 
                                throw new BadRequestException($"Softcopy of the {getDocumentReqApplicantDetail.DocumentReqType.Name} should be uploaded before check the 'Document Is Ready'");
                        }
                    }

                    if (paramChecklistStatus.IsChecked)
                    {
                        getDocumentReqApplicantDetail.ReceivedDateByStaff = _dateTime.ServerTime;
                        getDocumentReqApplicantDetail.IdBinusianReceiver = AuthInfo.UserId;
                    }
                    else
                    {
                        getDocumentReqApplicantDetail.ReceivedDateByStaff = null;
                        getDocumentReqApplicantDetail.IdBinusianReceiver = null;
                    }

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

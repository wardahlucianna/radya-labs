using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestApprover;
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestType;
using BinusSchool.Document.FnDocument.DocumentRequest.DocumentRequestApprover;
using BinusSchool.Document.FnDocument.DocumentRequest.DocumentRequestType.Validator;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.DocumentDb.Entities;

namespace BinusSchool.Document.FnDocument.DocumentRequest.DocumentRequestType
{
    public class UpdateDocumentTypeStatusHandler : FunctionsHttpSingleHandler
    {
        private readonly IDocumentDbContext _dbContext;
        private readonly CheckAdminAccessByIdBinusianHandler _checkAdminAccessByIdBinusianHandler;

        public UpdateDocumentTypeStatusHandler(
            IDocumentDbContext dbContext,
            CheckAdminAccessByIdBinusianHandler checkAdminAccessByIdBinusianHandler)
        {
            _dbContext = dbContext;
            _checkAdminAccessByIdBinusianHandler = checkAdminAccessByIdBinusianHandler;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<UpdateDocumentTypeStatusRequest, UpdateDocumentTypeStatusValidator>();

            var updateStatus = await _dbContext.Entity<MsDocumentReqType>()
                                .FindAsync(param.IdDocumentReqType);

            if (updateStatus == null)
                throw new BadRequestException("Cannot find document request type");

            #region check authorization
            var checkAdminAccess = await _checkAdminAccessByIdBinusianHandler.CheckAdminAccessByIdBinusian(new CheckAdminAccessByIdBinusianRequest
            {
                IdSchool = updateStatus.IdSchool,
                IdBinusian = AuthInfo.UserId
            });

            if (!checkAdminAccess.HasAdminAccess)
                throw new BadRequestException("You are not authorized to perform this action");
            #endregion

            updateStatus.Status = param.ActiveStatus;

            _dbContext.Entity<MsDocumentReqType>().Update(updateStatus);
            await _dbContext.SaveChangesAsync(CancellationToken);

            return Request.CreateApiResult2();
        }
    }
}

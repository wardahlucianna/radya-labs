using System;
using System.Collections.Generic;
using System.Linq;
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
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Document.FnDocument.DocumentRequest.DocumentRequestType
{
    public class DeleteDocumentRequestTypeHandler : FunctionsHttpSingleHandler
    {
        private readonly IDocumentDbContext _dbContext;
        private readonly CheckAdminAccessByIdBinusianHandler _checkAdminAccessByIdBinusianHandler;

        public DeleteDocumentRequestTypeHandler(
            IDocumentDbContext dbContext,
            CheckAdminAccessByIdBinusianHandler checkAdminAccessByIdBinusianHandler)
        {
            _dbContext = dbContext;
            _checkAdminAccessByIdBinusianHandler = checkAdminAccessByIdBinusianHandler;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<DeleteDocumentRequestTypeRequest, DeleteDocumentRequestTypeValidator>();

            var getDocumentReqType = await _dbContext.Entity<MsDocumentReqType>()
                                        .Include(x => x.DocumentReqApplicantDetails)
                                        .Where(x => x.Id == param.IdDocumentReqType)
                                        .FirstOrDefaultAsync(CancellationToken);

            if (getDocumentReqType == null)
                throw new BadRequestException("Document Request Type not found");

            #region check authorization
            var checkAdminAccess = await _checkAdminAccessByIdBinusianHandler.CheckAdminAccessByIdBinusian(new CheckAdminAccessByIdBinusianRequest
            {
                IdSchool = getDocumentReqType.IdSchool,
                IdBinusian = AuthInfo.UserId
            });

            if (!checkAdminAccess.HasAdminAccess)
                throw new BadRequestException("You are not authorized to perform this action");
            #endregion

            if (getDocumentReqType.DocumentReqApplicantDetails != null && getDocumentReqType.DocumentReqApplicantDetails.Any())
                throw new BadRequestException("Cannot delete used document type");

            _dbContext.Entity<MsDocumentReqType>().Remove(getDocumentReqType);

            await _dbContext.SaveChangesAsync(CancellationToken);

            return Request.CreateApiResult2();
        }
    }
}

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
using BinusSchool.Document.FnDocument.DocumentRequest.DocumentRequestApprover.Validator;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.DocumentDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Document.FnDocument.DocumentRequest.DocumentRequestApprover
{
    public class AddDocumentRequestApproverHandler : FunctionsHttpSingleHandler
    {
        private readonly IDocumentDbContext _dbContext;

        public AddDocumentRequestApproverHandler(IDocumentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<AddDocumentRequestApproverRequest, AddDocumentRequestApproverValidator>();

            // validate existing approver
            var hasDuplicateApprover = _dbContext.Entity<MsDocumentReqApprover>()
                                        .Where(x => x.IdSchool == param.IdSchool &&
                                                    x.IdBinusian == param.IdBinusian)
                                        .Any();

            if (hasDuplicateApprover)
                throw new BadRequestException("Cannot insert the same approver as on the existing list of approvers");

            var addNewApprover = new MsDocumentReqApprover
            {
                Id = Guid.NewGuid().ToString(),
                IdSchool = param.IdSchool,
                IdBinusian = param.IdBinusian
            };

            _dbContext.Entity<MsDocumentReqApprover>().Add(addNewApprover);
            await _dbContext.SaveChangesAsync(CancellationToken);

            return Request.CreateApiResult2();
        }
    }
}

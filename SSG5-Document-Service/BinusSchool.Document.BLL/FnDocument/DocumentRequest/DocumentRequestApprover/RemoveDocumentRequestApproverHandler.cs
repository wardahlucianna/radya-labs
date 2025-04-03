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
using BinusSchool.Document.FnDocument.DocumentRequest.Validator;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.DocumentDb.Entities;

namespace BinusSchool.Document.FnDocument.DocumentRequest.DocumentRequestApprover
{
    public class RemoveDocumentRequestApproverHandler : FunctionsHttpSingleHandler
    {
        private readonly IDocumentDbContext _dbContext;

        public RemoveDocumentRequestApproverHandler(IDocumentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<RemoveDocumentRequestApproverRequest, RemoveDocumentRequestApproverValidator>();

            var getApprover = await _dbContext.Entity<MsDocumentReqApprover>()
                                .FindAsync(param.IdDocumentReqApprover);

            if(getApprover == null)
                throw new BadRequestException("Approver not found");

            // validate whether the approver is the only one in the school
            var approverCounter = _dbContext.Entity<MsDocumentReqApprover>()
                                    .Where(x => x.IdSchool == getApprover.IdSchool)
                                    .Count();

            if (approverCounter <= 1)
                throw new BadRequestException("Unable to remove because approver is the only one in the school");

            var deleteApprover = _dbContext.Entity<MsDocumentReqApprover>().Remove(getApprover);
            await _dbContext.SaveChangesAsync(CancellationToken);

            return Request.CreateApiResult2();
        }
    }
}

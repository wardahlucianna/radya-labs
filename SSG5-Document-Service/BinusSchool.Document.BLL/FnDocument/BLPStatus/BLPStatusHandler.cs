using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Data.Model.Document.FnDocument.BLPStatus;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.DocumentDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Document.FnDocument.BLPStatus
{
    public class BLPStatusHandler : FunctionsHttpCrudHandler
    {
        private readonly IDocumentDbContext _dbContext;
        public BLPStatusHandler(IDocumentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            throw new NotImplementedException();
        }

        protected override Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            throw new NotImplementedException();
        }

        protected override async Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            var param = Request.ValidateParams<GetBLPStatusRequest>(nameof(GetBLPStatusRequest.IdSchool));

            IReadOnlyList<IItemValueVm> returnResult;

            returnResult = await _dbContext.Entity<LtBLPStatus>()
                                            .Where(a => a.IdSchool == param.IdSchool)
                                            .Select(a => new GetBLPStatusResult
                                            {
                                                Id = a.Id,
                                                Description = a.ShortName
                                            })
                                            .OrderBy(a => a.Description)
                                            .ToListAsync();

            return Request.CreateApiResult2(returnResult);
        }

        protected override Task<ApiErrorResult<object>> PostHandler()
        {
            throw new NotImplementedException();
        }

        protected override Task<ApiErrorResult<object>> PutHandler()
        {
            throw new NotImplementedException();
        }
    }
}

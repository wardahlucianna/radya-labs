using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestType;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.DocumentDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Document.FnDocument.DocumentRequest.DocumentRequestType
{
    public class GetDocumentRequestFieldTypeHandler : FunctionsHttpSingleHandler
    {
        private readonly IDocumentDbContext _dbContext;

        public GetDocumentRequestFieldTypeHandler(IDocumentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetDocumentRequestFieldTypeRequest>();

            var items = await _dbContext.Entity<LtDocumentReqFieldType>()
                            .Select(x => new GetDocumentRequestFieldTypeResult
                            {
                                IdDocumentReqFieldType = x.Id,
                                Type = x.Type,
                                HasOption = x.HasOption
                            })
                            .ToListAsync(CancellationToken);

            return Request.CreateApiResult2(items as object);
        }
    }
}

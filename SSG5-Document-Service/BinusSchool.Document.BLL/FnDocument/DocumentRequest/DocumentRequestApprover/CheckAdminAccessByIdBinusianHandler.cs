using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestApprover;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.DocumentDb.Entities;

namespace BinusSchool.Document.FnDocument.DocumentRequest.DocumentRequestApprover
{
    public class CheckAdminAccessByIdBinusianHandler : FunctionsHttpSingleHandler
    {
        private readonly IDocumentDbContext _dbContext;

        public CheckAdminAccessByIdBinusianHandler(IDocumentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<CheckAdminAccessByIdBinusianRequest>(
                            nameof(CheckAdminAccessByIdBinusianRequest.IdSchool),
                            nameof(CheckAdminAccessByIdBinusianRequest.IdBinusian));

            var result = await CheckAdminAccessByIdBinusian(param);

            return Request.CreateApiResult2(result as object);
        }

        public async Task<CheckAdminAccessByIdBinusianResult> CheckAdminAccessByIdBinusian(CheckAdminAccessByIdBinusianRequest param)
        {
            var hasAccess = _dbContext.Entity<MsDocumentReqApprover>()
                            .Where(x => x.IdSchool == param.IdSchool &&
                                        x.IdBinusian == param.IdBinusian)
                            .Any();

            var result = new CheckAdminAccessByIdBinusianResult
            {
                HasAdminAccess = hasAccess ? true : false
            };

            return result;
        }
    }
}

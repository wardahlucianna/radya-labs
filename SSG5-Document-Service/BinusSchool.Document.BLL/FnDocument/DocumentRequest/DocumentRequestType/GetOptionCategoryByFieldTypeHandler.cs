using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestType;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.DocumentDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Document.FnDocument.DocumentRequest.DocumentRequestType
{
    public class GetOptionCategoryByFieldTypeHandler : FunctionsHttpSingleHandler
    {
        private static readonly string[] _columns = new[]
        {
            "CategoryDescription"
        };

        private static readonly IDictionary<string, string> _aliasColumns = new Dictionary<string, string>
        {
            { _columns[0], "CategoryDescription" }
        };

        private readonly IDocumentDbContext _dbContext;

        public GetOptionCategoryByFieldTypeHandler(IDocumentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetOptionCategoryByFieldTypeRequest>(
                            nameof(GetOptionCategoryByFieldTypeRequest.IdSchool),
                            nameof(GetOptionCategoryByFieldTypeRequest.IdDocumentReqFieldType));

            var predicate = PredicateBuilder.True<MsDocumentReqOptionCategory>();

            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x
                    //=> EF.Functions.Like(x.Name, $"%{param.Search}%")
                    => EF.Functions.Like(x.CategoryDescription, param.SearchPattern())
                    );

            var items = await _dbContext.Entity<MsDocumentReqOptionCategory>()
                            .Where(predicate)
                            .Where(x => x.IdSchool == param.IdSchool &&
                                        x.IdDocumentReqFieldType == param.IdDocumentReqFieldType)
                            .Select(x => new GetOptionCategoryByFieldTypeResult
                            {
                                IdDocumentReqOptionCategory = x.Id,
                                CategoryDescription = x.CategoryDescription,
                                IsDefaultImportData = x.IsDefaultImportData
                            })
                            .OrderBy(x => x.CategoryDescription)
                            .OrderByDynamic(param, _aliasColumns)
                            .ToListAsync(CancellationToken);

            return Request.CreateApiResult2(items as object);
        }
    }
}

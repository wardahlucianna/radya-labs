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
    public class GetOptionsByOptionCategoryHandler : FunctionsHttpSingleHandler
    {
        private readonly IDocumentDbContext _dbContext;
        private readonly UpdateImportedDataOptionCategoryHandler _updateImportedDataOptionCategoryHandler;

        public GetOptionsByOptionCategoryHandler(
            IDocumentDbContext dbContext,
            UpdateImportedDataOptionCategoryHandler updateImportedDataOptionCategoryHandler)
        {
            _dbContext = dbContext;
            _updateImportedDataOptionCategoryHandler = updateImportedDataOptionCategoryHandler;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetOptionsByOptionCategoryRequest>(
                            nameof(GetOptionsByOptionCategoryRequest.IdDocumentReqOptionCategory));



            await _updateImportedDataOptionCategoryHandler.UpdateImportedDataOptionCategory(new UpdateImportedDataOptionCategoryRequest
            {
                IdDocumentReqOptionCategory = param.IdDocumentReqOptionCategory
            });

            var items = await _dbContext.Entity<MsDocumentReqOptionCategory>()
                                .Include(x => x.DocumentReqOptions)
                                .Where(x => x.Id == param.IdDocumentReqOptionCategory)
                                .Select(x => new GetOptionsByOptionCategoryResult
                                {
                                    IdDocumentReqOptionCategory = x.Id,
                                    CategoryName = x.CategoryDescription,
                                    IsDefaultImportData = x.IsDefaultImportData,
                                    OptionList = x.DocumentReqOptions
                                                    .Where(y => (y.Status == true ||
                                            (param.IdDocumentReqOptionChosenList == null ? false : param.IdDocumentReqOptionChosenList.Any(z => z == y.Id)))
                                                    )
                                                    .Select(y => new GetOptionsByOptionCategoryResult_Option
                                                    {
                                                        IdDocumentReqOption = y.Id,
                                                        OptionDescription = y.OptionDescription,
                                                        IsImportOption = y.IsImportOption,
                                                        CanDelete = y.IsImportOption ? false : true
                                                    })
                                                    .OrderBy(y => y.OptionDescription)
                                                    .ToList()
                                })
                                .FirstOrDefaultAsync(CancellationToken);
            
            return Request.CreateApiResult2(items as object);
        }
    }
}

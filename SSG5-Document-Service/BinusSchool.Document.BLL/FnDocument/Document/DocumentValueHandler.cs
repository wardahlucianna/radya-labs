using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Data.Abstractions;
using BinusSchool.Data.Api.Document.FnDocument;
using BinusSchool.Data.Api.School.FnSchool;
using BinusSchool.Data.Model.Document.FnDocument.Document;
using BinusSchool.Domain.Extensions;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.DocumentDb.Entities;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace BinusSchool.Document.FnDocument.Document
{
    public class DocumentValueHandler : FunctionsHttpSingleHandler
    {
        private readonly IDocumentDbContext _dbContext;
        private readonly IApiService<IMetadata> _metadataService;

        public DocumentValueHandler(IDocumentDbContext dbContext, IApiService<IMetadata> metadataService)
        {
            _dbContext = dbContext;
            _metadataService = metadataService;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.GetParams<SelectDocumentValueRequest>();
            var query = _dbContext.Entity<MsFormDoc>()
                   .Include(x => x.Form).ThenInclude(x => x.DocCategory)
                   .Where(x => x.Id == param.IdDocument)
                   .SearchByDynamic(param)
                   .OrderByDynamic(param);
            IReadOnlyList<IItemValueVm> items = default;
            var resp = await query.Select(x => x.JsonDocumentValue).FirstOrDefaultAsync(CancellationToken);
            var jsonValue = JsonConvert.DeserializeObject<Dictionary<object, object>>(resp);
            items = jsonValue.Select(x => new ItemValueVm(x.Key.ToString(), x.Value.ToString())).ToList();
            var count = param.CanCountWithoutFetchDb(items.Count)
                  ? items.Count
                  : await query.Select(x => x.Id).CountAsync(CancellationToken);
            return Request.CreateApiResult2(items as object, null);
        }
    }
}

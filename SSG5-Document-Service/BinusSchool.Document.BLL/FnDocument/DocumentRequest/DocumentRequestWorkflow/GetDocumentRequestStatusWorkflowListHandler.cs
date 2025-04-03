using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestWorkflow;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.DocumentDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Document.FnDocument.DocumentRequest.DocumentRequestWorkflow
{
    public class GetDocumentRequestStatusWorkflowListHandler : FunctionsHttpSingleHandler
    {
        private static readonly string[] _columns = new[] { "Description" };
        //private static readonly IDictionary<string, string> _aliasColumns = new Dictionary<string, string>
        //{
        //    { _columns[0], "name" },
        //};

        private readonly IDocumentDbContext _dbContext;

        public GetDocumentRequestStatusWorkflowListHandler(IDocumentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetDocumentRequestStatusWorkflowListRequest>(
                            nameof(GetDocumentRequestStatusWorkflowListRequest.IsFromParent));

            var predicate = PredicateBuilder.True<LtDocumentReqStatusWorkflow>();

            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x
                    //=> EF.Functions.Like(x.Name, $"%{param.Search}%")
                    => EF.Functions.Like(param.IsFromParent ? x.ParentDescription : x.StaffDescription, param.SearchPattern())
                    );

            var query = _dbContext.Entity<LtDocumentReqStatusWorkflow>()
                            .Where(predicate)
                            .Where(x => x.IdDocumentReqStatusWorkflow != DocumentRequestStatusWorkflow.InitialStatus);

            query = param.OrderBy switch
            {
                "Description" => param.OrderType == OrderType.Asc
                    ? query.OrderBy(x => param.IsFromParent ? x.ParentDescription : x.StaffDescription)
                    : query.OrderByDescending(x => param.IsFromParent ? x.ParentDescription : x.StaffDescription),
                _ => query.OrderByDynamic(param)
            };

            IReadOnlyList<IItemValueVm> items;
            if (param.Return == CollectionType.Lov)
                items = await query
                    .Select(x => new GetDocumentRequestStatusWorkflowListResult
                    {
                        Id = ((int)x.IdDocumentReqStatusWorkflow).ToString(),
                        IdDocumentReqStatusWorkflow = x.IdDocumentReqStatusWorkflow,
                        Description = param.IsFromParent ? x.ParentDescription : x.StaffDescription
                    })
                    .ToListAsync();
            else
                items = await query
                    .SetPagination(param)
                    .Select(x => new GetDocumentRequestStatusWorkflowListResult
                    {
                        IdDocumentReqStatusWorkflow = x.IdDocumentReqStatusWorkflow,
                        Description = param.IsFromParent ? x.ParentDescription : x.StaffDescription
                    })
                    .ToListAsync();

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x).CountAsync();

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(_columns));
        }
    }
}

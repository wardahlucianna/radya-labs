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
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.CreateDocumentRequest;
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestPayment;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.DocumentDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Document.FnDocument.DocumentRequest.DocumentRequestPayment
{
    public class GetPaymentMethodBySchoolHandler : FunctionsHttpSingleHandler
    {
        private static readonly string[] _columns = new[] { "Name" };
        //private static readonly IDictionary<string, string> _aliasColumns = new Dictionary<string, string>
        //{
        //    { _columns[0], "name" },
        //};

        private readonly IDocumentDbContext _dbContext;

        public GetPaymentMethodBySchoolHandler(IDocumentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetPaymentMethodBySchoolRequest>(
                            nameof(GetPaymentMethodBySchoolRequest.IdSchool));

            var predicate = PredicateBuilder.True<GetDefaultPICListResult>();
            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x
                    => EF.Functions.Like(x.Id, param.SearchPattern())
                    || EF.Functions.Like(x.PICName, param.SearchPattern())
                    || EF.Functions.Like(x.PICEmail, param.SearchPattern())
                    );

            var query = _dbContext.Entity<LtDocumentReqPaymentMethod>()
                            .Where(x => x.IdSchool == param.IdSchool &&
                                        (string.IsNullOrEmpty(param.IdDocumentReqPaymentMethod) ? true : x.Id == param.IdDocumentReqPaymentMethod)
                                        );

            query = param.OrderBy switch
            {
                "Name" => param.OrderType == OrderType.Asc
                    ? query.OrderBy(x => x.Name)
                    : query.OrderByDescending(x => x.Name),
                _ => query.OrderByDynamic(param)
            };

            IReadOnlyList<IItemValueVm> items;
            if (param.Return == CollectionType.Lov)
                items = await query
                    .Select(x => new ItemValueVm
                    {
                        Id = x.Id,
                        Description = x.Name
                    })
                    .ToListAsync();
            else
                items = await query
                    .SetPagination(param)
                    .Select(x => new GetPaymentMethodBySchoolResult
                    {
                        IdDocumentReqPaymentMethod = x.Id,
                        Name = x.Name,
                        UsingManualVerification = x.UsingManualVerification,
                        IsVirtualAccount = x.IsVirtualAccount,
                        AccountNumber = x.AccountNumber,
                        DescriptionHTML = x.DescriptionHTML,
                        ImageUrl = x.ImageUrl
                    })
                    .ToListAsync();

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.Id).CountAsync();

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(_columns));
        }
    }
}

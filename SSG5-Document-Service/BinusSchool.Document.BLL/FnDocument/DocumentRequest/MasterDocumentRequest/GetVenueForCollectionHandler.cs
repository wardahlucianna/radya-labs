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
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.MasterDocumentRequest;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.DocumentDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Document.FnDocument.DocumentRequest.MasterDocumentRequest
{
    public class GetVenueForCollectionHandler : FunctionsHttpSingleHandler
    {
        private static readonly string[] _columns = new[] { "Description" };

        private readonly IDocumentDbContext _dbContext;

        public GetVenueForCollectionHandler(IDocumentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetVenueForCollectionRequest>(
                            nameof(GetVenueForCollectionRequest.IdSchool));

            var predicate = PredicateBuilder.True<MsDocumentReqCollectionVenue>();

            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x
                    //=> EF.Functions.Like(x.Name, $"%{param.Search}%")
                    => EF.Functions.Like(x.Venue.Description, param.SearchPattern())
                    );

            var query = _dbContext.Entity<MsDocumentReqCollectionVenue>()
                            .Include(x => x.Venue)
                            .Where(predicate)
                            .Where(x => x.IdSchool == param.IdSchool);

            query = param.OrderBy switch
            {
                "Description" => param.OrderType == OrderType.Asc
                    ? query.OrderBy(x => x.Venue.Description)
                    : query.OrderByDescending(x => x.Venue.Description),
                _ => query.OrderByDynamic(param)
            };

            IReadOnlyCollection<IItemValueVm> items;
            if (param.Return == CollectionType.Lov)
                items = await query
                            .Select(x => new ItemValueVm
                            {
                                Id = x.IdVenue,
                                Description = x.Venue.Description
                            })
                            .ToListAsync(CancellationToken);
            else
                items = await query
                            .SetPagination(param)
                            .Select(x => new GetVenueForCollectionResult
                            {
                                Id = x.IdVenue,
                                Description = x.Venue.Description
                            })
                            .ToListAsync(CancellationToken);

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x).CountAsync();

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(_columns));
        }
    }
}

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
using BinusSchool.Data.Model.School.FnSchool.VenueWithBuilding;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using Microsoft.EntityFrameworkCore;
using NPOI.SS.Formula.Functions;
using Org.BouncyCastle.Utilities;

namespace BinusSchool.School.FnSchool.VenueWithBuilding
{
    public class GetVenueWithBuildingHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _dbContext;
        public GetVenueWithBuildingHandler(ISchoolDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetVenueWithBuildingRequest>(nameof(GetVenueWithBuildingRequest.IdSchool));

            var predicate = PredicateBuilder.Create<MsVenue>(x => x.Building.IdSchool == param.IdSchool);

            if (!string.IsNullOrWhiteSpace(param.Search))
            {
                predicate = predicate.And(x
                    => EF.Functions.Like(x.Description, param.SearchPattern())
                    || EF.Functions.Like(x.Building.Description, param.SearchPattern()));
            }

            var query = _dbContext.Entity<MsVenue>()
                .Where(predicate)
                .OrderByDynamic(param);

            IReadOnlyList<IItemValueVm> items;
            if (param.Return == CollectionType.Lov)
                items = await query
                    .Select(x => new ItemValueVm
                    {
                        Id = x.Id,
                        Description = x.Description + " (" + x.Building.Description + ")"
                    })
                    .ToListAsync(CancellationToken);
            else
                items = await query
                    .SetPagination(param)
                    .Select(x => new ItemValueVm
                    {
                        Id = x.Id,
                        Description = x.Description + " (" + x.Building.Description + ")"
                    })
                    .ToListAsync(CancellationToken);
            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.Id).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count));
        }
    }
}


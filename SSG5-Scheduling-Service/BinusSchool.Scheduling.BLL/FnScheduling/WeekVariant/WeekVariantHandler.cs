using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.WeekVariant;
using BinusSchool.Domain.Extensions;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.WeekVariant
{
    public class WeekVariantHandler : FunctionsHttpCrudHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public WeekVariantHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            throw new System.NotImplementedException();
        }

        protected override Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            throw new System.NotImplementedException();
        }

        protected override async Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            var param = Request.ValidateParams<CollectionSchoolRequest>(nameof(CollectionSchoolRequest.IdSchool));
            var columns = new[] { "code", "description" };

            var predicate = PredicateBuilder.Create<MsWeekVariant>(x => true); // TODO: add filter by idSchool
            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x
                    => EF.Functions.Like(x.Code, param.SearchPattern())
                    || EF.Functions.Like(x.Description, param.SearchPattern()));

            var query = _dbContext.Entity<MsWeekVariant>()
                .SearchByIds(param)
                .Where(predicate)
                .OrderByDynamic(param);

            IReadOnlyList<IItemValueVm> items = default;
            if (param.Return == CollectionType.Lov)
                items = await query
                    .Select(x => new CodeWithIdVm
                    {
                        Id = x.Id,
                        Code = x.Code,
                        Description = string.Join(", ", x.WeekVarianDetails.Select(x => x.Week.Description).OrderBy(x=> x))
                    })
                    .ToListAsync(CancellationToken);
            else
                items = await query
                    .SetPagination(param)
                    .Select(x => new GetWeekVariantResult
                    {
                        Id = x.Id,
                        Code = x.Code,
                        Description = x.Description,
                        Weeks = x.WeekVarianDetails.Select(y => new CodeWithIdVm
                        {
                            Id = y.IdWeek,
                            Code = y.Week.Code,
                            Description = y.Week.Description
                        })
                    })
                    .ToListAsync(CancellationToken);
            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.Id).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items, param.CreatePaginationProperty(count).AddColumnProperty(columns));
        }

        protected override Task<ApiErrorResult<object>> PostHandler()
        {
            throw new System.NotImplementedException();
        }

        protected override Task<ApiErrorResult<object>> PutHandler()
        {
            throw new System.NotImplementedException();
        }
    }
}

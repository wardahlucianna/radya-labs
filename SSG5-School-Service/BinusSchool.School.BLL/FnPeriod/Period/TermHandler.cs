using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.School.FnPeriod.Period;
using BinusSchool.Domain.Extensions;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.School.FnPeriod.Period
{
    public class TermHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _dbContext;
        private readonly IServiceProvider _provider;

        public TermHandler(ISchoolDbContext dbContext , IServiceProvider provider)
        {
            _dbContext = dbContext;
            _provider = provider;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<SelectTermRequest>(nameof(SelectTermRequest.IdGrade));
            var query = _dbContext.Entity<MsPeriod>()
                .Where(x => param.IdGrade.Any(y => y == x.IdGrade))
                .SearchByDynamic(param)
                .OrderByDynamic(param);
            if (!string.IsNullOrEmpty(param.Search))
            {
                query = query.Where(x=>x.Code.Contains(param.Search) || x.Description.Contains(param.Search));
            }
            IReadOnlyList<IItemValueVm> items;
            if (param.Return == CollectionType.Lov)
                items = await query
                    .Select(x => new ItemValueVm(x.Id, x.Description))
                    .ToListAsync(CancellationToken);
            else
                items = await query
                    .SetPagination(param)
                    .Select(x => new SelectTermResult
                    {
                        Id = x.Id,
                        Code = x.Code,
                        Description = x.Description,
                        Semester = x.Semester
                    })
                    .ToListAsync(CancellationToken);
            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.Id).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items as object, null);
        }
    }
}

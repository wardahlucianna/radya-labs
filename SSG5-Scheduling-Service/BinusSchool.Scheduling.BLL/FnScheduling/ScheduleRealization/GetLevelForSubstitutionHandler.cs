using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnSchedule.ScheduleRealization;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.ScheduleRealization
{
    public class GetLevelForSubstitutionHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        public GetLevelForSubstitutionHandler(
            ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected async override Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetLevelForSubstitutionRequest>(nameof(GetLevelForSubstitutionRequest.IdAcademicYear));

            var predicate = PredicateBuilder.Create<TrScheduleRealization>(x => x.Homeroom.IdAcademicYear == param.IdAcademicYear);

            var query = _dbContext.Entity<TrScheduleRealization>()
                                 .Include(x => x.Homeroom).ThenInclude(x => x.Grade).ThenInclude(x => x.Level)
                                 .Where(predicate)
                                 .Select(x => new { x.Homeroom.Grade.Level.Id, x.Homeroom.Grade.Level.Description })
                                 .Distinct()
                                 .OrderBy(x => x.Description);

            IReadOnlyList<IItemValueVm> items;
            items = await query
                .Select(x => new ItemValueVm(x.Id, $"{x.Description}"))
                .Distinct()
                .ToListAsync(CancellationToken);

            items = items
                    .Select(x => new ItemValueVm(
                    x.Id,
                    x.Description
                )
            ).ToList();
           
            return Request.CreateApiResult2(items as object);
        }
    }
}

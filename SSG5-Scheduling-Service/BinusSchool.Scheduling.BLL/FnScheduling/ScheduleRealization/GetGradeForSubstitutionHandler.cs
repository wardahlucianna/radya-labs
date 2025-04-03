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
    public class GetGradeForSubstitutionHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        public GetGradeForSubstitutionHandler(
            ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected async override Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetGradeForSubstitutionRequest>(nameof(GetGradeForSubstitutionRequest.IdAcademicYear)
                                                                               ,nameof(GetGradeForSubstitutionRequest.IdLevel));

            var predicate = PredicateBuilder.Create<TrScheduleRealization>(x => x.Homeroom.IdAcademicYear == param.IdAcademicYear && x.Homeroom.Grade.IdLevel == param.IdLevel);

            var query = _dbContext.Entity<TrScheduleRealization>()
                                 .Include(x => x.Homeroom).ThenInclude(x => x.Grade)
                                 .Where(predicate)
                                 .Select(x => new { x.Homeroom.Grade.Id, x.Homeroom.Grade.Description })
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

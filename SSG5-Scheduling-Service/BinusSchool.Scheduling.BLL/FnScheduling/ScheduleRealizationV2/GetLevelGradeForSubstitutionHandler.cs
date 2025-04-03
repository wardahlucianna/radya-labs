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
using BinusSchool.Data.Model.Scheduling.FnSchedule.ScheduleRealizationV2;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.ScheduleRealizationV2
{
    public class GetLevelGradeForSubstitutionHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        public GetLevelGradeForSubstitutionHandler(
            ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected async override Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetLevelGradeForSubstitutionRequest>(nameof(GetLevelGradeForSubstitutionRequest.IdAcademicYear));

            var predicate = PredicateBuilder.Create<TrScheduleRealization2>(x => x.IdAcademicYear == param.IdAcademicYear);

            if(param.IsLevel)
            {
                var query = _dbContext.Entity<TrScheduleRealization2>()
                                    .Include(x => x.Level)
                                    .Where(predicate)
                                    .Select(x => new { x.IdLevel, x.Level.Description, x.Level.OrderNumber })
                                    .Distinct()
                                    .OrderBy(x => x.OrderNumber);

                IReadOnlyList<IItemValueVm> items;
                items = await query
                    .Select(x => new ItemValueVm(x.IdLevel, $"{x.Description}"))
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
            else
            {
                if(!String.IsNullOrEmpty(param.IdLevel))
                {
                    predicate = predicate.And(x => x.IdLevel == param.IdLevel);
                }

                var query = _dbContext.Entity<TrScheduleRealization2>()
                                    .Include(x => x.Grade)
                                    .Where(predicate)
                                    .Select(x => new { x.IdGrade, x.Grade.Description, x.Grade.OrderNumber })
                                    .Distinct()
                                    .OrderBy(x => x.OrderNumber);

                IReadOnlyList<IItemValueVm> items;
                items = await query
                    .Select(x => new ItemValueVm(x.IdGrade, $"{x.Description}"))
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
}

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
using BinusSchool.Data.Model.Scheduling.FnSchedule.GenerateSchedule;
using BinusSchool.Domain.Extensions;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.GenerateSchedule
{
    public class GetGenerateScheduleGradeV2Handler : FunctionsHttpSingleHandler
    {
        private static readonly Lazy<string[]> _requiredParams = new Lazy<string[]>(new[]
        {
            nameof(GetGenerateScheduleGradeRequest.IdGrade)
        });
        private static readonly Lazy<string[]> _columns = new Lazy<string[]>(new[]
        {
            "code", "ascTimetable.name", "startPeriod", "endPeriod"
        });

        private readonly ISchedulingDbContext _dbContext;

        public GetGenerateScheduleGradeV2Handler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetGenerateScheduleGradeRequest>(_requiredParams.Value);
            var predicate = PredicateBuilder.Create<MsScheduleLesson>(x
                => x.IdGrade == param.IdGrade
                && x.IsGenerated);
            if (!string.IsNullOrEmpty(param.IdAscTimetable))
                predicate = predicate.And(x => x.GeneratedScheduleGrade.GeneratedSchedule.IdAscTimetable == param.IdAscTimetable);
          
            var query = _dbContext.Entity<MsScheduleLesson>()
                                    .Include(x => x.GeneratedScheduleGrade)
                                        .ThenInclude(x => x.GeneratedSchedule)
                                            .ThenInclude(x => x.TrAscTimetable)
                                  .Include(x => x.Week)
                .Where(predicate)
                .SearchByIds(param)
                .GroupBy(x => new
                {
                    x.ClassID,
                    x.GeneratedScheduleGrade.GeneratedSchedule.TrAscTimetable.Name,
                    x.GeneratedScheduleGrade.GeneratedSchedule.TrAscTimetable.Id
                });

            if (!string.IsNullOrEmpty(param.OrderBy))
            {
                query = param.OrderBy switch
                {
                    "code" => param.OrderType == OrderType.Asc
                        ? query.OrderBy(x => x.Key.ClassID)
                        : query.OrderByDescending(x => x.Key.ClassID),
                    "ascTimetable.name" => param.OrderType == OrderType.Asc
                        ? query.OrderBy(x => x.Key.Name)
                        : query.OrderByDescending(x => x.Key.Name),
                    "startPeriod" => param.OrderType == OrderType.Asc
                        ? query.OrderBy(x => x.Min(y => y.ScheduleDate))
                        : query.OrderByDescending(x => x.Min(y => y.ScheduleDate)),
                    "endPeriod" => param.OrderType == OrderType.Asc
                        ? query.OrderBy(x => x.Max(y => y.ScheduleDate))
                        : query.OrderByDescending(x => x.Max(y => y.ScheduleDate)),
                    _ => query
                };
            }

            IReadOnlyList<IItemValueVm> items = default;
            var count = 0;
            if (param.Return == CollectionType.Lov)
            {
                items = await query
                    .Select(x => new CodeWithIdVm(x.Key.Id, x.Key.ClassID))
                    .ToListAsync(CancellationToken);

                count = items.Count();
            }
            else
            {
                var results = await query
                    .If(string.IsNullOrWhiteSpace(param.Search), x => x.SetPagination(param))
                    .Select(x => new GetGenerateScheduleGradeResult
                    {
                        Description = x.Key.ClassID,
                        Code = x.Key.ClassID,
                        StartPeriod = x.Min(x => x.ScheduleDate),
                        EndPeriod = x.Max(x => x.ScheduleDate),
                        AscTimetable = new NameValueVm
                        {
                            Id = x.Key.Id,
                            Name = x.Key.Name
                        },
                    })
                    .ToListAsync(CancellationToken);

                if (!string.IsNullOrWhiteSpace(param.Search))
                    items = results
                        .Where(x
                            => x.Code.Contains(param.Search, StringComparison.OrdinalIgnoreCase)
                            || x.AscTimetable.Name.Contains(param.Search, StringComparison.OrdinalIgnoreCase)
                            || $"{x.StartPeriod:dd MMM yyyy} - {x.EndPeriod:dd MMM yyyy}".Contains(param.Search, StringComparison.OrdinalIgnoreCase))
                        .SetPagination(param)
                        .ToList();
                else
                    items = results;

                count = param.CanCountWithoutFetchDb(items.Count)
                    ? items.Count
                    : string.IsNullOrWhiteSpace(param.Search)
                        ? await query.Select(x => x.Key).CountAsync(CancellationToken)
                        : results.Count(x
                            => x.Code.Contains(param.Search, StringComparison.OrdinalIgnoreCase)
                            || x.AscTimetable.Name.Contains(param.Search, StringComparison.OrdinalIgnoreCase)
                            || $"{x.StartPeriod:dd MMM yyyy} - {x.EndPeriod:dd MMM yyyy}".Contains(param.Search, StringComparison.OrdinalIgnoreCase));
            }

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(_columns.Value));
        }
    }
}

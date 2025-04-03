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
using BinusSchool.Data.Model.Scheduling.FnSchedule.GenerateSchedule;
using BinusSchool.Domain.Extensions;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.GenerateSchedule
{
    public class GetGenerateScheduleGradeHistoryHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public GetGenerateScheduleGradeHistoryHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetGenerateScheduleGradeHistoryRequest>(
                nameof(GetGenerateScheduleGradeHistoryRequest.ClassId),
                //nameof(GetGenerateScheduleGradeHistoryRequest.IdAscTimetable),
                //nameof(GetGenerateScheduleGradeHistoryRequest.StartPeriod),
                //nameof(GetGenerateScheduleGradeHistoryRequest.EndPeriod),
                nameof(GetGenerateScheduleGradeHistoryRequest.IdGrade)
                );
            var columns = new[] { "code", "ascTimetable.name", "startPeriod", "endPeriod" };
            var aliasColumns = new Dictionary<string, string>()
            {
                { columns[0], "GeneratedScheduleStudent.GeneratedScheduleGrade.GeneratedSchedule.TrAscTimetable.Name" },
                { columns[1], "StartPeriod" },
                { columns[2], "DateIn" }
            };

            var predicate = PredicateBuilder.Create<TrGeneratedScheduleLesson>(x
                => x.GeneratedScheduleStudent.GeneratedScheduleGrade.IdGrade == param.IdGrade
                //&& x.GeneratedScheduleStudent.GeneratedScheduleGrade.GeneratedSchedule.IdAscTimetable == param.IdAscTimetable
                //&& x.StartPeriod.Date >= param.StartPeriod.Date
                //&& x.EndPeriod.Date <= param.EndPeriod.Date
                && x.ClassID == param.ClassId);
            if (!string.IsNullOrEmpty(param.IdAscTimetable))
            {
                predicate = predicate.And(x => x.GeneratedScheduleStudent.GeneratedScheduleGrade.GeneratedSchedule.IdAscTimetable == param.IdAscTimetable);
            }

            var query = _dbContext.Entity<TrGeneratedScheduleLesson>()
                .Include(x => x.GeneratedScheduleStudent)
                .ThenInclude(x => x.GeneratedScheduleGrade)
                .ThenInclude(x => x.GeneratedSchedule)
                .ThenInclude(x => x.TrAscTimetable)
                .IgnoreQueryFilters()
                .Where(predicate)
                .OrderByDynamic(param, aliasColumns);

            IReadOnlyList<IItemValueVm> items = default;
            if (param.Return == CollectionType.Lov)
            {
                items = await query
                    .Select(x => new CodeWithIdVm(x.Id, x.ClassID))
                    .ToListAsync(CancellationToken);
            }
            else
            {
                items = await query
                    .GroupBy(x=> new
                    {
                        x.ClassID,
                        x.DateIn,
                        x.GeneratedScheduleStudent.GeneratedScheduleGrade.GeneratedSchedule.TrAscTimetable.Name,
                        x.GeneratedScheduleStudent.GeneratedScheduleGrade.GeneratedSchedule.TrAscTimetable.Id
                    })
                    .OrderByDescending(x=>x.Key.DateIn)
                    .Select(x => new GetGenerateScheduleGradeHistoryResult
                    {
                        StartPeriod = x.Min(x => x.ScheduleDate),
                        EndPeriod = x.Max(x => x.ScheduleDate),
                        AscTimetable = new NameValueVm
                        {
                            Id = x.Key.Id,
                            Name = x.Key.Name
                        },
                        DateIn = x.Key.DateIn
                    })
                    .ToListAsync(CancellationToken);
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.Id).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(columns));
        }
    }
}

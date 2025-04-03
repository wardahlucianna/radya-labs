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
    public class GetGenerateScheduleGradeHandler : FunctionsHttpSingleHandler
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

        public GetGenerateScheduleGradeHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetGenerateScheduleGradeRequest>(_requiredParams.Value);
            var predicate = PredicateBuilder.Create<TrGeneratedScheduleLesson>(x
                => x.GeneratedScheduleStudent.GeneratedScheduleGrade.IdGrade == param.IdGrade
                && x.IsGenerated);
            if (!string.IsNullOrEmpty(param.IdAscTimetable))
                predicate = predicate.And(x => x.GeneratedScheduleStudent.GeneratedScheduleGrade.GeneratedSchedule.IdAscTimetable == param.IdAscTimetable);
            //if (!string.IsNullOrEmpty(param.Search))
            //    predicate = predicate.And(x
            //        => EF.Functions.Like(x.ClassID, param.SearchPattern())
            //        || EF.Functions.Like(x.GeneratedScheduleStudent.GeneratedScheduleGrade.GeneratedSchedule.TrAscTimetable.Name, param.SearchPattern())
            //        || EF.Functions.Like(Convert.ToString(x.StartPeriod), param.SearchPattern())
            //        || EF.Functions.Like(Convert.ToString(x.EndPeriod), param.SearchPattern()));

            var query = _dbContext.Entity<TrGeneratedScheduleLesson>()
                                  .Include(x => x.GeneratedScheduleStudent)
                                    .ThenInclude(x => x.GeneratedScheduleGrade)
                                        .ThenInclude(x => x.GeneratedSchedule)
                                            .ThenInclude(x => x.TrAscTimetable)
                                  .Include(x => x.Week)
                .Where(predicate)
                .SearchByIds(param)
                .GroupBy(x => new
                {
                    x.ClassID,
                    x.GeneratedScheduleStudent.GeneratedScheduleGrade.GeneratedSchedule.TrAscTimetable.Name,
                    x.GeneratedScheduleStudent.GeneratedScheduleGrade.GeneratedSchedule.TrAscTimetable.Id
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
                        // Week = new CodeWithIdVm
                        // {
                        //     Id = x.IdWeek,
                        //     Code = x.Week.Code,
                        //     Description = x.Week.Description
                        // },
                        // Histories = _dbContext.Entity<TrGeneratedScheduleLesson>()
                        // .Include(x=>x.GeneratedScheduleStudent)
                        //     .ThenInclude(x=>x.GeneratedScheduleGrade)
                        //         .ThenInclude(x=>x.GeneratedSchedule)
                        //             .ThenInclude(x=>x.TrAscTimetable)
                        //     .Where(y
                        //         => y.ClassID == x.ClassID
                        //         && y.GeneratedScheduleStudent.GeneratedScheduleGrade.GeneratedSchedule.IdAscTimetable == x.GeneratedScheduleStudent.GeneratedScheduleGrade.GeneratedSchedule.IdAscTimetable
                        //         && !y.IsGenerated)
                        //     .OrderByDescending(y => y.DateIn)
                        //     .Select(y => new GenerateScheduleGradeHistory
                        //     {
                        //         Id = y.Id,
                        //         Name = y.GeneratedScheduleStudent.GeneratedScheduleGrade.GeneratedSchedule.TrAscTimetable.Name,
                        //         StartPeriod = y.StartPeriod,
                        //         EndPeriod = y.EndPeriod,
                        //         CreatedDate = y.DateIn ?? DateTime.MinValue
                        //     })
                        //     .ToList()
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

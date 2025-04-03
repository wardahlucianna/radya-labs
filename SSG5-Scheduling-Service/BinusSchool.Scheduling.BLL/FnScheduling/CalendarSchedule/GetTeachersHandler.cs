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
using BinusSchool.Data.Model.Scheduling.FnSchedule.CalendarSchedule;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.CalendarSchedule
{
    public class GetTeachersHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        public GetTeachersHandler(
            ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected async override Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetUsersRequest>(nameof(GetUsersRequest.IdHomeroom));
            var columns = new[] { "description" };

            //var predicate = PredicateBuilder.Create<TrGeneratedScheduleLesson>(x => x.IdHomeroom == param.IdHomeroom);

            //if (!string.IsNullOrWhiteSpace(param.Search))
            //    predicate = predicate.And(x => EF.Functions.Like(x.TeacherName, $"%{param.Search}%"));

            //if (param.Ids != null && param.Ids.Any())
            //    predicate = predicate.And(x => param.Ids.Contains(x.IdUser));

            //var query = _dbContext.Entity<TrGeneratedScheduleLesson>()
            //                      .Where(predicate)
            //                      .Select(x => new { x.IdUser, x.TeacherName })
            //                      .Distinct();

            var predicate = PredicateBuilder.Create<MsLessonTeacher>(x => x.Lesson.LessonPathways.Any(y => y.HomeroomPathway.IdHomeroom == param.IdHomeroom));

            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x => EF.Functions.Like(x.Staff.FirstName, $"%{param.Search}%")
                                            || EF.Functions.Like(x.Staff.FirstName, $"%{param.Search}%"));

            var query = _dbContext.Entity<MsLessonTeacher>()
                                  .Where(predicate)
                                  .Select(x => new { 
                                      IdUser = x.IdUser,
                                      FirstName = string.IsNullOrEmpty(x.Staff.FirstName) ? x.Staff.LastName : x.Staff.FirstName,
                                      LastName = string.IsNullOrEmpty(x.Staff.FirstName) ? x.Staff.FirstName : x.Staff.LastName,
                                  })
                                  .Distinct();

            if (!string.IsNullOrEmpty(param.OrderBy))
            {
                //if (param.OrderType == OrderType.Asc)
                //    query = query.OrderBy(x => x.TeacherName);
                //else
                //    query = query.OrderByDescending(x => x.TeacherName);
                if (param.OrderType == OrderType.Asc)
                    query = query.OrderBy(x => x.FirstName).ThenBy(x => x.LastName);
                else
                    query = query.OrderByDescending(x => x.FirstName).ThenByDescending(x => x.LastName);
            }
            else
                //query = query.OrderBy(x => x.TeacherName);
                query = query.OrderBy(x => x.FirstName).ThenBy(x => x.LastName);

            IReadOnlyList<IItemValueVm> items;
            if (param.Return == CollectionType.Lov)
                items = await query
                    //.Select(x => new ItemValueVm(x.IdUser, x.TeacherName))
                    .Select(x => new ItemValueVm(x.IdUser, $"{x.FirstName} {x.LastName}"))
                    .ToListAsync(CancellationToken);
            else
                items = await query
                    .SetPagination(param)
                    //.Select(x => new ItemValueVm(x.IdUser, x.TeacherName))
                    .Select(x => new ItemValueVm(x.IdUser, $"{x.FirstName} {x.LastName}"))
                    .ToListAsync(CancellationToken);
            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.CountAsync(CancellationToken);

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(columns));
        }
    }
}

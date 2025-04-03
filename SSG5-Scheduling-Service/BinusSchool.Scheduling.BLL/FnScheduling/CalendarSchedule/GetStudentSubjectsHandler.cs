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
    public class GetStudentSubjectsHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        public GetStudentSubjectsHandler(
            ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected async override Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetUserSubjectsRequest>(nameof(GetUserSubjectsRequest.IdUser));
            var columns = new[] { "description" };

            var predicate = PredicateBuilder.Create<TrGeneratedScheduleLesson>(x => x.GeneratedScheduleStudent.IdStudent == param.IdUser);

            if (!string.IsNullOrWhiteSpace(param.IdHomeroom))
                predicate = predicate.And(x => x.IdHomeroom == param.IdHomeroom);

            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x => EF.Functions.Like(x.SubjectName, $"%{param.Search}%"));

            if (param.Ids != null && param.Ids.Any())
                predicate = predicate.And(x => param.Ids.Contains(x.IdSubject));

            var query = _dbContext.Entity<TrGeneratedScheduleLesson>()
                                  .Include(x => x.GeneratedScheduleStudent)
                                  .Where(predicate)
                                  .Select(x => new { x.IdSubject, x.SubjectName })
                                  .Distinct();

            if (!string.IsNullOrEmpty(param.OrderBy))
            {
                if (param.OrderType == OrderType.Asc)
                    query = query.OrderBy(x => x.SubjectName);
                else
                    query = query.OrderByDescending(x => x.SubjectName);
            }
            else
                query = query.OrderBy(x => x.SubjectName);

            IReadOnlyList<IItemValueVm> items;
            if (param.Return == CollectionType.Lov)
                items = await query
                    .Select(x => new ItemValueVm(x.IdSubject, x.SubjectName))
                    .ToListAsync(CancellationToken);
            else
                items = await query
                    .SetPagination(param)
                    .Select(x => new ItemValueVm(x.IdSubject, x.SubjectName))
                    .ToListAsync(CancellationToken);
            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.CountAsync(CancellationToken);

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(columns));
        }
    }
}

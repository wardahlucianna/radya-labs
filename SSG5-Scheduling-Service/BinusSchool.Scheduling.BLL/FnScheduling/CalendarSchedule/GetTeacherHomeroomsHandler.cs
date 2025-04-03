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
    public class GetTeacherHomeroomsHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        public GetTeacherHomeroomsHandler(
            ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected async override Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetTeacherHomeroomsRequest>(nameof(GetTeacherHomeroomsRequest.IdUser));
            var columns = new[] { "description" };

            var predicate = PredicateBuilder.Create<TrGeneratedScheduleLesson>(x => x.IdUser == param.IdUser);

            if (!string.IsNullOrWhiteSpace(param.IdAcadYear))
                predicate = predicate.And(x => x.Homeroom.IdAcademicYear == param.IdAcadYear);

            if (!string.IsNullOrWhiteSpace(param.IdGrade))
                predicate = predicate.And(x => x.Homeroom.IdGrade == param.IdGrade);

            if (param.Semester.HasValue)
                predicate = predicate.And(x => x.Homeroom.Semester == param.Semester.Value);

            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x => EF.Functions.Like(x.HomeroomName, $"%{param.Search}%"));

            if (param.Ids != null && param.Ids.Any())
                predicate = predicate.And(x => param.Ids.Contains(x.IdHomeroom));

            var query = _dbContext.Entity<TrGeneratedScheduleLesson>()
                                  .Include(x => x.Homeroom).ThenInclude(e=>e.Grade)
                                  .Include(x => x.Homeroom).ThenInclude(e=>e.GradePathwayClassroom).ThenInclude(e=>e.Classroom)
                                  .Where(predicate)
                                  .Select(x => new { IdHomeroom = x.IdHomeroom, HomeroomName = x.Homeroom.Grade.Code + x.Homeroom.GradePathwayClassroom.Classroom.Code }).Distinct();

            if (!string.IsNullOrEmpty(param.OrderBy))
            {
                if (param.OrderType == OrderType.Asc)
                    query = query.OrderBy(x => x.HomeroomName);
                else
                    query = query.OrderByDescending(x => x.HomeroomName);
            }
            else
                query = query.OrderBy(x => x.HomeroomName);

            IReadOnlyList<IItemValueVm> items;
            if (param.Return == CollectionType.Lov)
                items = await query
                    .Select(x => new CodeWithIdVm(x.IdHomeroom, x.HomeroomName,x.HomeroomName))
                    .ToListAsync(CancellationToken);
            else
                items = await query
                    .SetPagination(param)
                    .Select(x => new CodeWithIdVm(x.IdHomeroom, x.HomeroomName, x.HomeroomName))
                    .ToListAsync(CancellationToken);
            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.CountAsync(CancellationToken);

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(columns));
        }
    }
}

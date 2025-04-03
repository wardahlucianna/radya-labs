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
    public class GetStudentsHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        public GetStudentsHandler(
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
            //    predicate = predicate.And(x => EF.Functions.Like(x.GeneratedScheduleStudent.Student.FirstName, $"%{param.Search}%")
            //                                   || EF.Functions.Like(x.GeneratedScheduleStudent.Student.LastName, $"%{param.Search}%"));

            //if (param.Ids != null && param.Ids.Any())
            //    predicate = predicate.And(x => param.Ids.Contains(x.IdUser));

            //var query = _dbContext.Entity<TrGeneratedScheduleLesson>()
            //                      .Include(x => x.GeneratedScheduleStudent).ThenInclude(x => x.Student)
            //                      .Where(predicate)
            //                      //.Select(x => new { x.GeneratedScheduleStudent.IdStudent, x.GeneratedScheduleStudent.Student.FirstName, x.GeneratedScheduleStudent.Student.LastName })
            //                      .Select(x => new GetUsersResult
            //                      {
            //                          IdStudent = x.GeneratedScheduleStudent.IdStudent,
            //                          FirstName = string.IsNullOrEmpty(x.GeneratedScheduleStudent.Student.FirstName) ? x.GeneratedScheduleStudent.Student.LastName : x.GeneratedScheduleStudent.Student.FirstName,
            //                          LastName = string.IsNullOrEmpty(x.GeneratedScheduleStudent.Student.FirstName)? x.GeneratedScheduleStudent.Student.FirstName : x.GeneratedScheduleStudent.Student.LastName,
            //                      })
            //                      .Distinct();

            var predicate = PredicateBuilder.Create<MsHomeroomStudent>(x => x.IdHomeroom == param.IdHomeroom);

            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x => EF.Functions.Like((string.IsNullOrWhiteSpace(x.Student.FirstName) ? "" : x.Student.FirstName.Trim() + " ") +
                                        (string.IsNullOrWhiteSpace(x.Student.MiddleName) ? "" : x.Student.MiddleName.Trim() + " ") +
                                        (string.IsNullOrWhiteSpace(x.Student.LastName) ? "" : x.Student.LastName.Trim()), param.SearchPattern()
                                        ));

            var query = _dbContext.Entity<MsHomeroomStudent>()
                                  .Include(x => x.HomeroomStudentEnrollments)
                                  .Where(predicate)
                                  .Select(x => new GetUsersResult
                                  {
                                      IdStudent = x.IdStudent,
                                      FirstName = string.IsNullOrEmpty(x.Student.FirstName) ? x.Student.LastName : x.Student.FirstName,
                                      LastName = string.IsNullOrEmpty(x.Student.FirstName) ? x.Student.FirstName : x.Student.LastName,
                                  });

            if (!string.IsNullOrEmpty(param.OrderBy))
            {
                if (param.OrderType == OrderType.Asc)
                    query = query.OrderBy(x => x.FirstName).ThenBy(x => x.LastName);
                else
                    query = query.OrderByDescending(x => x.FirstName).ThenByDescending(x => x.LastName);
            }
            else
                query = query.OrderBy(x => x.FirstName).ThenBy(x => x.LastName);

            IReadOnlyList<IItemValueVm> items;
            if (param.Return == CollectionType.Lov)
                items = await query
                    .Select(x => new ItemValueVm(x.IdStudent, $"{x.FirstName} {x.LastName}"))
                    .ToListAsync(CancellationToken);
            else
                items = await query
                    .SetPagination(param)
                    .Select(x => new ItemValueVm(x.IdStudent, $"{x.FirstName} {x.LastName}"))
                    .ToListAsync(CancellationToken);
            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.CountAsync(CancellationToken);

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(columns));
        }
    }
}

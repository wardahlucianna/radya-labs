using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnSchedule.GenerateSchedule;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.GenerateSchedule
{
    public class GetGradeByAscTimetableHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public GetGradeByAscTimetableHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetGradeByAscTimetableRequest>(
               nameof(GetGradeByAscTimetableRequest.IdAscTimetable));

            var query = _dbContext.Entity<MsGrade>()
                .Include(x => x.Lessons).ThenInclude(x => x.AscTimetableLessons).ThenInclude(x => x.AscTimetable)
                .Where(x => x.Lessons.Any(x => x.AscTimetableLessons.Any(x => x.AscTimetable.Id == param.IdAscTimetable)));

            var result = await query
                .Select(x => new GetGradeByAscTimetableResult
                {
                    Id = x.Id,
                    Code = x.Code,
                    Description = x.Description
                })
                .OrderBy(x => x.Code)
                .ToListAsync(CancellationToken);

            return Request.CreateApiResult2(result as object);
        }
    }
}

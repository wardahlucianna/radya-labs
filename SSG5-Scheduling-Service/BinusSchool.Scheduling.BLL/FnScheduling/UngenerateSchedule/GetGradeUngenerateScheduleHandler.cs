using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Extensions;
using BinusSchool.Data.Model.Scheduling.FnSchedule.UngenerateSchedule;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.UngenerateSchedule
{
    public class GetGradeUngenerateScheduleHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        public GetGradeUngenerateScheduleHandler(ISchedulingDbContext schoolDbContext)
        {
            _dbContext = schoolDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = Request.ValidateParams<GetGradeUngenerateScheduleRequest>();

            var listGrade = await _dbContext.Entity<MsScheduleLesson>()
                            .Include(e => e.Grade)
                            .Where(e => e.GeneratedScheduleGrade.GeneratedSchedule.IdAscTimetable == body.IdAscTimetable && e.IdAcademicYear == body.IdAcademicYear)
                            .GroupBy(e => new
                            {
                                IdGrade = e.Grade.Id,
                                Grade = e.Grade.Description,
                                GradeOrderNumber = e.Grade.OrderNumber,
                            })
                            .Select(e=>e.Key)
                            .ToListAsync(CancellationToken);

            var items = listGrade
                            .OrderBy(e=>e.GradeOrderNumber)
                            .Select(e => new ItemValueVm
                            {
                                Id = e.IdGrade,
                                Description = e.Grade,
                            })
                            .ToList();

            return Request.CreateApiResult2(items as object);
        }
    }
}

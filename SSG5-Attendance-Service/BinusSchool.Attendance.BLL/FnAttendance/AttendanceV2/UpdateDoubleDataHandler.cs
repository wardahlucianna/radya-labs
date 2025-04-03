using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceV2;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Attendance.FnAttendance.AttendanceV2
{
    public class UpdateDoubleDataHandler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;
        public UpdateDoubleDataHandler(IAttendanceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<UpdateDoubleDataRequest>();

            var listAttendanceEntryGroup = await _dbContext.Entity<TrAttendanceEntryV2>()
                                        .Include(e => e.ScheduleLesson)
                                        .Include(e => e.HomeroomStudent)
                                        .Where(e => e.ScheduleLesson.IdAcademicYear == param.IdAcademicYear && e.ScheduleLesson.IsGenerated == true)
                                        .GroupBy(e => new
                                        {
                                            e.ScheduleLesson.ScheduleDate,
                                            e.IdScheduleLesson,
                                            e.HomeroomStudent.IdStudent
                                        })
                                        .Select(e => new
                                        {
                                            e.Key.ScheduleDate,
                                            e.Key.IdScheduleLesson,
                                            e.Key.IdStudent,
                                            total = e.Count()
                                        })
                                        .Where(e=>e.total>1)
                                        .ToListAsync(CancellationToken);

            var listIdScheduleLesson = listAttendanceEntryGroup.Select(e=>e.IdScheduleLesson).Distinct().ToList();

            var listAttendanceEntry = await _dbContext.Entity<TrAttendanceEntryV2>()
                                      .Include(e => e.ScheduleLesson)
                                      .Include(e => e.HomeroomStudent)
                                      .Where(e => e.ScheduleLesson.IdAcademicYear == param.IdAcademicYear && e.ScheduleLesson.IsGenerated == true && listIdScheduleLesson.Contains(e.IdScheduleLesson))
                                      .ToListAsync(CancellationToken);

            foreach (var item in listAttendanceEntryGroup)
            {
                var dataEntry = listAttendanceEntry
                                .Where(e => e.ScheduleLesson.ScheduleDate == item.ScheduleDate && e.HomeroomStudent.IdStudent == item.IdStudent && e.IdScheduleLesson == item.IdScheduleLesson)
                                .OrderBy(e => e.DateIn)
                                .ToList();

                var lastIdAttendanceEntry = dataEntry.Select(e=>e.IdAttendanceEntry).LastOrDefault();

                var updateAttendance = dataEntry.Where(e => e.IdAttendanceEntry != lastIdAttendanceEntry).ToList();
                updateAttendance.ForEach(e=>e.IsActive = false);

                _dbContext.Entity<TrAttendanceEntryV2>().UpdateRange(updateAttendance);
            }

            await _dbContext.SaveChangesAsync(CancellationToken);

            return Request.CreateApiResult2();
        }
    }
}

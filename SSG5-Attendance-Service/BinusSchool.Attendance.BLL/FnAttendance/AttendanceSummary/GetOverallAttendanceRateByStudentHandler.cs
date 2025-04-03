using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummary;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Attendance.FnAttendance.AttendanceSummary
{
    public class GetOverallAttendanceRateByStudentHandler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;
        public GetOverallAttendanceRateByStudentHandler(IAttendanceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {

            var param = Request.ValidateParams<GetOverallAttendanceRateByStudentRequest>(nameof(GetOverallAttendanceRateByStudentRequest.IdStudent),
                                                                                        nameof(GetOverallAttendanceRateByStudentRequest.IdGrade));

            var schedules = await _dbContext.Entity<TrGeneratedScheduleLesson>()
                                            .Include(x => x.Homeroom)
                                            .Include(x => x.AttendanceEntries).ThenInclude(x => x.AttendanceMappingAttendance).ThenInclude(x => x.Attendance)
                                            .Include(x => x.GeneratedScheduleStudent)
                                            .Where(x => x.IsGenerated
                                            && x.GeneratedScheduleStudent.IdStudent == param.IdStudent
                                            && x.Homeroom.Semester == ((param.Semester == null || param.Semester == 0) ? x.Homeroom.Semester : param.Semester)
                                            && x.Homeroom.IdGrade == param.IdGrade)
                                            .ToListAsync();


            var result = schedules.GroupBy(x => x.Homeroom.Semester)
                                  .Select(x => new GetOverallAttendanceRateByStudentResult
                                  {
                                      Semester = x.Key,
                                      TotalSession = x.Count(),
                                      TotalSessionSubmitted = x.Count(y => y.AttendanceEntries.Any(z => z.Status == AttendanceEntryStatus.Submitted)),
                                      Present = x.Count(y => y.AttendanceEntries.Any(z => z.AttendanceMappingAttendance.Attendance.AttendanceCategory == AttendanceCategory.Present && z.AttendanceMappingAttendance.Attendance.Code != "LT" && z.Status == AttendanceEntryStatus.Submitted)),
                                      ExcusedAbsent = x.Count(y => y.AttendanceEntries.Any(z => z.AttendanceMappingAttendance.Attendance.AbsenceCategory == AbsenceCategory.Excused && z.Status == AttendanceEntryStatus.Submitted)),
                                      UnexcusedAbsent = x.Count(y => y.AttendanceEntries.Any(z => z.AttendanceMappingAttendance.Attendance.AbsenceCategory == AbsenceCategory.Unexcused && z.Status == AttendanceEntryStatus.Submitted)),
                                      Late = x.Count(y => y.AttendanceEntries.Any(z => z.AttendanceMappingAttendance.Attendance.Code == "LT" && z.Status == AttendanceEntryStatus.Submitted)),
                                      PresenceRate = Math.Round(((Convert.ToDecimal(x.Count(y => y.AttendanceEntries.Any(z => (z.AttendanceMappingAttendance.Attendance.AttendanceCategory == AttendanceCategory.Present || z.AttendanceMappingAttendance.Attendance.Code == "LT" || z.AttendanceMappingAttendance.Attendance.AbsenceCategory == AbsenceCategory.Excused) && z.Status == AttendanceEntryStatus.Submitted))) - Convert.ToDecimal(x.Count(y => y.AttendanceEntries.Any(z => z.AttendanceMappingAttendance.Attendance.AbsenceCategory == AbsenceCategory.Unexcused && z.Status == AttendanceEntryStatus.Submitted))))
                                                            / Convert.ToDecimal(x.Count(y => y.AttendanceEntries.Any(z => z.Status == AttendanceEntryStatus.Submitted))) * 100), 2),
                                      PunctualityRate = Math.Round(((Convert.ToDecimal(x.Count(y => y.AttendanceEntries.Any(z => (z.AttendanceMappingAttendance.Attendance.AttendanceCategory == AttendanceCategory.Present || z.AttendanceMappingAttendance.Attendance.AbsenceCategory == AbsenceCategory.Excused) && z.Status == AttendanceEntryStatus.Submitted))) - Convert.ToDecimal(x.Count(y => y.AttendanceEntries.Any(z => z.AttendanceMappingAttendance.Attendance.AbsenceCategory == AbsenceCategory.Unexcused && z.Status == AttendanceEntryStatus.Submitted))))
                                                            / Convert.ToDecimal(x.Count(y => y.AttendanceEntries.Any(z => z.Status == AttendanceEntryStatus.Submitted))) * 100), 2),

                                  }).ToList();

          //   PunctualityRate = (double)(x.Count() - (x.Count(y => y.AttendanceEntries.Any(z => z.AttendanceMappingAttendance.Attendance.Code == "LT" && z.Status == AttendanceEntryStatus.Submitted)) +
          //x.Count(y => y.AttendanceEntries.Any(z => z.AttendanceMappingAttendance.Attendance.AttendanceCategory == AttendanceCategory.Absent && z.Status == AttendanceEntryStatus.Submitted)))) / x.Count(),

            var smtList = await _dbContext.Entity<MsHomeroom>().Include(a => a.GradePathwayClassroom).ThenInclude(b => b.GradePathway)
                                .Where(a => a.GradePathwayClassroom.GradePathway.IdGrade == param.IdGrade)
                                .Select(a => a.Semester).Distinct().ToListAsync();

            var returnResult = (from a in smtList
                                join b in result on a equals b.Semester into gj
                                from ob in gj.DefaultIfEmpty()
                                select new GetOverallAttendanceRateByStudentResult()
                                {
                                    Semester = a,
                                    TotalSession = (ob != null ? ob.TotalSession : null),
                                    TotalSessionSubmitted = (ob != null ? ob.TotalSessionSubmitted : null),
                                    Present = (ob != null ? ob.Present : null),
                                    ExcusedAbsent = (ob != null ? ob.ExcusedAbsent : null),
                                    UnexcusedAbsent = (ob != null ? ob.UnexcusedAbsent : null),
                                    Late = (ob != null ? ob.Late : null),
                                    PresenceRate = (ob != null ? ob.PresenceRate : null),
                                    PunctualityRate = (ob != null ? ob.PunctualityRate : null)
                                })
                               .ToList();

            return Request.CreateApiResult2(returnResult as object);
        }
    }
}

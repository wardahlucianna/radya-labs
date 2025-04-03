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
    public class GetOverallTermAttendanceRateByStudentHandler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;
        public GetOverallTermAttendanceRateByStudentHandler(IAttendanceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetOverallTermAttendanceRateByStudentRequest>(nameof(GetOverallTermAttendanceRateByStudentRequest.IdStudent),
                                                                                    nameof(GetOverallTermAttendanceRateByStudentRequest.IdGrade));

            var Period = await _dbContext.Entity<MsPeriod>()
                        .Where(a => a.IdGrade == param.IdGrade)
                        .OrderBy(a => a.Code)
                        .ToListAsync();

            var schedules = await _dbContext.Entity<TrGeneratedScheduleLesson>()
                                            .Include(x => x.Homeroom)
                                            .Include(x => x.AttendanceEntries).ThenInclude(x => x.AttendanceMappingAttendance).ThenInclude(x => x.Attendance)
                                            .Include(x => x.GeneratedScheduleStudent)
                                            .Where(x => x.IsGenerated
                                            && x.GeneratedScheduleStudent.IdStudent == param.IdStudent                                    
                                            && x.Homeroom.IdGrade == param.IdGrade)
                                            .ToListAsync();
            List<GetOverallTermAttendanceRateByStudentResult> ReturnResult = new List<GetOverallTermAttendanceRateByStudentResult>();
            foreach (var itmPeriod in Period)
            {           
                string TermPeriod = new String(itmPeriod.Code.Where(Char.IsDigit).ToArray());
                if(Convert.ToInt32(TermPeriod) <= (param.Term == 0 || param.Term == null ? 4 : param.Term))
                {

                    var result = schedules.Where(a => itmPeriod.StartDate <= a.ScheduleDate && a.ScheduleDate <= itmPeriod.EndDate).ToList();
                    ReturnResult.Add(new GetOverallTermAttendanceRateByStudentResult
                    {
                        Term = Convert.ToInt32(TermPeriod),
                        TotalSession = result.Count(),
                        Present = result.Count(y => y.AttendanceEntries.Any(z => z.AttendanceMappingAttendance.Attendance.AttendanceCategory == AttendanceCategory.Present && z.AttendanceMappingAttendance.Attendance.Code != "LT" && z.Status == AttendanceEntryStatus.Submitted)),
                        ExcusedAbsent = result.Count(y => y.AttendanceEntries.Any(z => z.AttendanceMappingAttendance.Attendance.AbsenceCategory == AbsenceCategory.Excused && z.Status == AttendanceEntryStatus.Submitted)),
                        UnexcusedAbsent = result.Count(y => y.AttendanceEntries.Any(z => z.AttendanceMappingAttendance.Attendance.AbsenceCategory == AbsenceCategory.Unexcused && z.Status == AttendanceEntryStatus.Submitted)),
                        Late = result.Count(y => y.AttendanceEntries.Any(z => z.AttendanceMappingAttendance.Attendance.Code == "LT" && z.Status == AttendanceEntryStatus.Submitted)),
                        PresenceRate = (double)(result.Count() - result.Count(y => y.AttendanceEntries.Any(z => z.AttendanceMappingAttendance.Attendance.AttendanceCategory == AttendanceCategory.Absent && z.Status == AttendanceEntryStatus.Submitted))) / result.Count(),
                        PunctualityRate = (double)(result.Count() - (result.Count(y => y.AttendanceEntries.Any(z => z.AttendanceMappingAttendance.Attendance.Code == "LT" && z.Status == AttendanceEntryStatus.Submitted)) +
                                                            result.Count(y => y.AttendanceEntries.Any(z => z.AttendanceMappingAttendance.Attendance.AttendanceCategory == AttendanceCategory.Absent && z.Status == AttendanceEntryStatus.Submitted)))) / result.Count(),


                    });

                }
                else
                {
                    ReturnResult.Add(new GetOverallTermAttendanceRateByStudentResult
                    {
                        Term = Convert.ToInt32(TermPeriod),
                        TotalSession = null,
                        Present = null,
                        ExcusedAbsent = null,
                        UnexcusedAbsent = null,
                        Late = null,
                        PresenceRate = null,
                        PunctualityRate = null       
                    });;
                }        

            }

            return Request.CreateApiResult2(ReturnResult as object);
        }
    }
}

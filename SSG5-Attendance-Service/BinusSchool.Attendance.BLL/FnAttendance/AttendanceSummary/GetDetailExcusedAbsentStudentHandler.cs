using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummary;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Attendance.FnAttendance.AttendanceSummary
{
    public class GetDetailExcusedAbsentStudentHandler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;

        public GetDetailExcusedAbsentStudentHandler(IAttendanceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetDetailExcusedAbsentStudentRequest>(
                nameof(GetDetailExcusedAbsentStudentRequest.IdAcademicYear),
                nameof(GetDetailExcusedAbsentStudentRequest.StartDate),
                nameof(GetDetailExcusedAbsentStudentRequest.EndDate),
                nameof(GetDetailExcusedAbsentStudentRequest.IdStudent));
            var data = await _dbContext.Entity<TrAttendanceEntry>()
                .Include(x => x.GeneratedScheduleLesson)
                    .ThenInclude(x => x.GeneratedScheduleStudent)
                        .ThenInclude(x => x.Student)
                .Include(x => x.AttendanceMappingAttendance)
                    .ThenInclude(x => x.Attendance)
                .Where(x => x.GeneratedScheduleLesson.ScheduleDate.Date >= param.StartDate.Date)
                .Where(x => x.GeneratedScheduleLesson.ScheduleDate.Date <= param.EndDate.Date)
                .Where(x => x.GeneratedScheduleLesson.GeneratedScheduleStudent.IdStudent == param.IdStudent)
                .Where(x => param.ExcusedAbsenceCategory.HasValue ? 
                            x.AttendanceMappingAttendance.Attendance.ExcusedAbsenceCategory == param.ExcusedAbsenceCategory :
                            x.AttendanceMappingAttendance.Attendance.AbsenceCategory == Common.Model.Enums.AbsenceCategory.Excused)
                .Where(x => x.AttendanceMappingAttendance.Attendance.IdAcademicYear == param.IdAcademicYear)
                .Where(x => x.Status == Common.Model.Enums.AttendanceEntryStatus.Submitted)
                .OrderBy(x => x.DateIn.Value)
                .Select(x => new GetDetailExcusedAbsentStudentResult
                {
                    Date = x.GeneratedScheduleLesson.ScheduleDate,
                    SessionNo = x.GeneratedScheduleLesson.SessionID,
                    SubjectName = x.GeneratedScheduleLesson.SubjectName,
                    IdHomeroom = x.GeneratedScheduleLesson.IdHomeroom,
                    TeacherName = x.GeneratedScheduleLesson.TeacherName,
                    Attendance = x.AttendanceMappingAttendance.Attendance.Description,
                    Reason = x.Notes,
                    FileEvidence = x.FileEvidence
                }).ToListAsync();

            var newData = data.GroupBy(x => new { x.Date, x.SubjectName, x.SessionNo, x.IdHomeroom, x.Attendance, x.Reason, x.FileEvidence });

            var result = new List<GetDetailExcusedAbsentStudentResult>();

            foreach (var item in newData)
            {
                var datas = new GetDetailExcusedAbsentStudentResult
                {
                    Date = item.Key.Date,
                    SessionNo = item.Key.SessionNo,
                    SubjectName = item.Key.SubjectName,
                    IdHomeroom = item.Key.IdHomeroom,
                    Attendance = item.Key.Attendance,
                    Reason = item.Key.Reason,
                    FileEvidence = item.Key.FileEvidence,
                    TeacherName = string.Join(", ", item.Select(x => x.TeacherName).OrderBy(x => x))
                };

                result.Add(datas);
            }

            return Request.CreateApiResult2(result as object);
        }
    }
}

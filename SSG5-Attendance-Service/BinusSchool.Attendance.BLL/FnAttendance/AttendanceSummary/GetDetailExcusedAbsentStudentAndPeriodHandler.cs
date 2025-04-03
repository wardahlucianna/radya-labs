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
    public class GetDetailExcusedAbsentStudentAndPeriodHandler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;

        public GetDetailExcusedAbsentStudentAndPeriodHandler(IAttendanceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetDetailExcusedAbsentStudentAndPeriodRequest>(
                nameof(GetDetailExcusedAbsentStudentAndPeriodRequest.IdAcademicYear),
                nameof(GetDetailExcusedAbsentStudentAndPeriodRequest.Semester),
                nameof(GetDetailExcusedAbsentStudentAndPeriodRequest.IdStudent));
            var data = await _dbContext.Entity<TrAttendanceEntry>()
                .Include(x => x.GeneratedScheduleLesson)
                    .ThenInclude(x => x.GeneratedScheduleStudent)
                        .ThenInclude(x => x.Student)
                .Include(x => x.AttendanceMappingAttendance)
                    .ThenInclude(x => x.Attendance)
                .Include(x => x.GeneratedScheduleLesson)
                    .ThenInclude(x => x.Homeroom)
                .Where(x => x.GeneratedScheduleLesson.Homeroom.Semester == param.Semester)
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
                }).ToListAsync(CancellationToken);
            return Request.CreateApiResult2(data as object);
        }
    }
}

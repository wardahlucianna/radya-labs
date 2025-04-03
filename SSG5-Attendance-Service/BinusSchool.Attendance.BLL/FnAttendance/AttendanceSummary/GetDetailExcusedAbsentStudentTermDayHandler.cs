using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummary;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using BinusSchool.Persistence.AttendanceDb.Entities.Employee;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Attendance.FnAttendance.AttendanceSummary
{
    public class GetDetailExcusedAbsentStudentTermDayHandler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;

        public GetDetailExcusedAbsentStudentTermDayHandler(IAttendanceDbContext dbContext)
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
                .GroupBy(x => new
                {
                    x.GeneratedScheduleLesson.ScheduleDate,
                    x.GeneratedScheduleLesson.IdHomeroom,
                    x.AttendanceMappingAttendance.Attendance.Description,
                    x.Notes,
                    x.DateIn,
                    x.FileEvidence
                })
                .OrderBy(x => x.Key.DateIn.Value)
                .Select(x => new GetDetailExcusedAbsentStudentResult
                {
                    Date = x.Key.ScheduleDate,
                    IdHomeroom = x.Key.IdHomeroom,
                    Attendance = x.Key.Description,
                    Reason = x.Key.Notes,
                    FileEvidence = x.Key.FileEvidence
                }).ToListAsync();

            var homeroomIds = data.Select(x => x.IdHomeroom).Distinct();
            var homeroomTeachers = await _dbContext.Entity<MsHomeroomTeacher>()
                                            .Include(x => x.Staff)
                                            .Include(x => x.TeacherPosition)
                                                .ThenInclude(x => x.LtPosition)
                                            .Where(x => homeroomIds.Contains(x.IdHomeroom)
                                                       && x.TeacherPosition.LtPosition.Code == PositionConstant.ClassAdvisor)
                                            .Select(x => new
                                            {
                                                x.IdHomeroom,
                                                x.IdBinusian,
                                                Name = $"{x.Staff.FirstName} {x.Staff.LastName}"
                                            })
                                            .ToListAsync(CancellationToken);

            var resultData = data.Select(x => new GetDetailExcusedAbsentStudentResult
            {
                Date = x.Date,
                SessionNo = x.SessionNo,
                SubjectName = x.SubjectName,
                IdHomeroom = x.IdHomeroom,
                TeacherName = homeroomTeachers.Where(y => y.IdHomeroom == x.IdHomeroom).Select(y => y.Name).FirstOrDefault(),
                Attendance = x.Attendance,
                Reason = x.Reason,
                FileEvidence = x.FileEvidence
            }).ToList();

            return Request.CreateApiResult2(resultData as object);
        }
    }
}

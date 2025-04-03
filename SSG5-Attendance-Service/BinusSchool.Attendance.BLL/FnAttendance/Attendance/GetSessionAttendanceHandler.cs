using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.Attendance.FnAttendance.Attendance;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using BinusSchool.Persistence.AttendanceDb.Entities.Student;
using BinusSchool.Persistence.AttendanceDb.Entities.User;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Attendance.FnAttendance.Attendance
{
    public class GetSessionAttendanceHandler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;
        public GetSessionAttendanceHandler(IAttendanceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected async override Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetAttendanceRequest>(nameof(GetAttendanceRequest.IdAcademicYear),
                                                                     nameof(GetAttendanceRequest.IdUser),
                                                                     nameof(GetAttendanceRequest.Semester),
                                                                     nameof(GetAttendanceRequest.Date),
                                                                     nameof(GetAttendanceRequest.IdSchool));

            List<SessionAttendanceResult> result = new List<SessionAttendanceResult>();
            var allowIsAttendanceEntryByClassId = await _dbContext.Entity<MsLessonTeacher>()
                .Include(x => x.Lesson)
                .Where(x => x.IdUser == param.IdUser)
                .Where(x => x.Lesson.IdAcademicYear == param.IdAcademicYear && x.Lesson.Semester == param.Semester)
                .Where(x => x.IsAttendance)
                .Select(x => x.Lesson.Id)
                .ToListAsync(CancellationToken);
            var data = await _dbContext.Entity<TrGeneratedScheduleLesson>()
                                       .Include(x => x.GeneratedScheduleStudent)
                                       .Include(x => x.AttendanceEntries).ThenInclude(x => x.AttendanceMappingAttendance).ThenInclude(x => x.Attendance)
                                       .Include(x => x.Homeroom)
                                            .ThenInclude(x => x.Grade)
                                                .ThenInclude(x => x.Level)
                                                    .ThenInclude(x => x.MappingAttendances)
                                       .Where(x => x.IsGenerated
                                                   && x.ScheduleDate.Date == param.Date.Date
                                                   && x.Homeroom.IdAcademicYear == param.IdAcademicYear
                                                   && x.Homeroom.Semester == param.Semester
                                                   && (x.IdUser == param.IdUser || allowIsAttendanceEntryByClassId.Contains(x.IdLesson))
                                                   && x.Homeroom.Grade.Level.MappingAttendances.Any(y => y.AbsentTerms == AbsentTerm.Session))
                                       .ToListAsync(CancellationToken);

            var checkStudentStatus = await _dbContext.Entity<TrStudentStatus>().Select(x => new {x.IdStudent, x.StartDate, x.EndDate, x.IdStudentStatus, x.CurrentStatus, x.ActiveStatus, x.IdAcademicYear})
                .Where(x => x.IdAcademicYear == param.IdAcademicYear && (x.StartDate == param.Date.Date || x.EndDate == param.Date.Date 
                    || (x.StartDate < param.Date.Date
                        ? x.EndDate != null ? (x.EndDate > param.Date.Date && x.EndDate < param.Date.Date) || x.EndDate > param.Date.Date : x.StartDate <= param.Date
                        : x.EndDate != null ? ((param.Date.Date > x.StartDate && param.Date.Date < x.EndDate) || param.Date.Date > x.EndDate) : x.StartDate <= param.Date)) && x.CurrentStatus == "A" && x.ActiveStatus == false)
                .ToListAsync();

            if(checkStudentStatus != null)
            {
                data = data.Where(x => !checkStudentStatus.Select(z=>z.IdStudent).ToList().Contains(x.GeneratedScheduleStudent.IdStudent)).ToList();
            }

            if (param.IdSchool == "1")
            {
                var data2 = new List<TrGeneratedScheduleLesson>();

                foreach (var item in data.GroupBy(x => new { x.GeneratedScheduleStudent.IdStudent , x.IdSession }).ToList())
                {
                    var dataComparison = data.Where(x => x.GeneratedScheduleStudent.IdStudent == item.Key.IdStudent && x.IdSession == item.Key.IdSession).ToList();
                    if (dataComparison.Any(x => x.AttendanceEntries.Count == 1))
                    {
                        data2.Add(dataComparison.Where(x => x.AttendanceEntries.Count > 0).FirstOrDefault());
                    }
                    else
                    {
                        data2.AddRange(dataComparison);
                    }
                }
                data = data2;
            }

            if (data.Count != 0)
            {
                if (param.IdSchool != "1")
                {
                    var dataFull = data;
                    if (data.Where(x => x.IdUser == param.IdUser).Count() > 0)
                    {
                        var selectTeacher = param.IdUser;
                        data = data.Where(x => x.IdUser == selectTeacher).ToList();
                    }
                    else
                    {
                        var listTeacher = data.GroupBy(x => x.IdUser).Select(x => x.Key).ToList();

                        data = data.Where(x => listTeacher.Contains(x.IdUser)).ToList();
                    }
                    foreach (var checkLesson in allowIsAttendanceEntryByClassId)
                    {
                        if (param.IdSchool != "2")
                        {
                            if (!data.Any(x => x.IdLesson == checkLesson))
                            {
                                foreach (var dataAdjust in dataFull.Where(x => x.IdLesson == checkLesson).ToList())
                                {
                                    data.Add(dataAdjust);
                                }
                            }
                        }
                    }
                }
 
                // var listTeacher = data.GroupBy(x => x.IdUser).Select(x=> x.Key).ToList();

                // data = data.Where(x => listTeacher.Contains(x.IdUser)).ToList();

                // var selectTeacher = data.First().IdUser;

                // data = data.Where(x => x.IdUser == selectTeacher).ToList();
            }

            result = data.GroupBy(x => new { x.IdSubject, x.SubjectName, x.IdSession, x.SessionID, x.ClassID })
                            .Select(x => new SessionAttendanceResult
                            {
                                Subject = new ItemValueVm
                                {
                                    Id = x.Key.IdSubject,
                                    Description = x.Key.SubjectName
                                },
                                Session = new ItemValueVm
                                {
                                    Id = x.Key.IdSession,
                                    Description = x.Key.SessionID
                                },
                                ClassId = x.Key.ClassID,
                                TotalStudent = data.Where(y => y.IdSubject == x.Key.IdSubject && y.SubjectName == x.Key.SubjectName && y.IdSession == x.Key.IdSession && y.SessionID == x.Key.SessionID && y.ClassID == x.Key.ClassID).Count(),
                                Pending = x.Count(x => x.AttendanceEntries.Any(y => y.Status == AttendanceEntryStatus.Pending)),
                                Submitted = x.Count(x => x.AttendanceEntries.Any(y => y.Status == AttendanceEntryStatus.Submitted)),
                                Unsubmitted = x.Count(x => !x.AttendanceEntries.Any()),
                                UnexcusedAbsence = x.Count(x => x.AttendanceEntries.Any(y => y.AttendanceMappingAttendance.Attendance.AbsenceCategory == AbsenceCategory.Unexcused && y.Status == AttendanceEntryStatus.Submitted)),
                                LastSavedBy = x.Where(x => x.AttendanceEntries.Any()).Any() ? (x.Where(x => x.AttendanceEntries.Any()).OrderByDescending(x => x.AttendanceEntries.Max(y => y.DateIn)).First().AttendanceEntries.OrderByDescending(x => x.DateUp).Select(x => x.UserUp).First() ??
                                                                                                 x.Where(x => x.AttendanceEntries.Any()).OrderByDescending(x => x.AttendanceEntries.Max(y => y.DateIn)).First().AttendanceEntries.OrderByDescending(x => x.DateIn).Select(x => x.UserIn).First()) : null,
                                LastSavedAt = x.Where(x => x.AttendanceEntries.Any()).Any() ? (x.Where(x => x.AttendanceEntries.Any()).OrderByDescending(x => x.AttendanceEntries.Max(y => y.DateIn)).First().AttendanceEntries.OrderByDescending(x => x.DateUp).Select(x => x.DateUp).First() ??
                                                                                                 x.Where(x => x.AttendanceEntries.Any()).OrderByDescending(x => x.AttendanceEntries.Max(y => y.DateIn)).First().AttendanceEntries.OrderByDescending(x => x.DateIn).Select(x => x.DateIn).First()) : null,
                            }).OrderBy(x => int.TryParse(x.Session.Description, out var sessionNumber) ? sessionNumber : 0)
                            .ToList();

            var userIds = result.Select(x => x.LastSavedBy).Distinct().ToList();
            var users = await _dbContext.Entity<MsUser>()
                                        .Where(x => userIds.Contains(x.Id))
                                        .Select(x => new { x.Id, x.DisplayName })
                                        .ToListAsync();

            foreach (var item in result)
            {
                if (!string.IsNullOrEmpty(item.LastSavedBy))
                {
                    item.LastSavedBy = users.Where(x => x.Id == item.LastSavedBy).Select(x => x.DisplayName).FirstOrDefault();
                }

            }
            return Request.CreateApiResult2(result as object);
        }
    }
}

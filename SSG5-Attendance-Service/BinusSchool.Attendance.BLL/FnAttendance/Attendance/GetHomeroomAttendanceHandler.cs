using System;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.Attendance.FnAttendance.Attendance;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using BinusSchool.Persistence.AttendanceDb.Entities.User;
using BinusSchool.Persistence.AttendanceDb.Entities.Student;
using Microsoft.EntityFrameworkCore;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using System.Collections.Generic;
using BinusSchool.Persistence.AttendanceDb.Entities.School;

namespace BinusSchool.Attendance.FnAttendance.Attendance
{
    public class GetHomeroomAttendanceHandler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;
        public GetHomeroomAttendanceHandler(IAttendanceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected async override Task<ApiErrorResult<object>> Handler()
        {
            FillConfiguration();

            var param = Request.ValidateParams<GetAttendanceRequest>(nameof(GetAttendanceRequest.IdAcademicYear),
                                                                     nameof(GetAttendanceRequest.IdUser),
                                                                     nameof(GetAttendanceRequest.Semester),
                                                                     nameof(GetAttendanceRequest.Date));

            var checkStudentStatus = await _dbContext.Entity<TrStudentStatus>().Select(x => new {x.IdStudent, x.StartDate, x.EndDate, x.IdStudentStatus, x.CurrentStatus, x.ActiveStatus, x.IdAcademicYear})
                .Where(x => x.IdAcademicYear == param.IdAcademicYear && (x.StartDate == param.Date.Date || x.EndDate == param.Date.Date 
                    || (x.StartDate < param.Date.Date
                        ? x.EndDate != null ? (x.EndDate > param.Date.Date && x.EndDate < param.Date.Date) || x.EndDate > param.Date.Date : x.StartDate <= param.Date
                        : x.EndDate != null ? ((param.Date.Date > x.StartDate && param.Date.Date < x.EndDate) || param.Date.Date > x.EndDate) : x.StartDate <= param.Date)) && x.CurrentStatus == "A" && x.ActiveStatus == false)
                .ToListAsync();

            var data = await _dbContext.Entity<TrGeneratedScheduleLesson>()
                                       .Include(x => x.GeneratedScheduleStudent)
                                       .Include(x => x.Homeroom).ThenInclude(x => x.HomeroomTeachers)
                                       .Include(x => x.Homeroom).ThenInclude(x => x.GradePathwayClassroom).ThenInclude(x => x.GradePathway).ThenInclude(x => x.Grade).ThenInclude(x => x.Level).ThenInclude(x => x.MappingAttendances)
                                       .Where(x => x.IsGenerated
                                                   && x.ScheduleDate == param.Date
                                                   && x.Homeroom.IdAcademicYear == param.IdAcademicYear
                                                   && x.Homeroom.Semester == param.Semester
                                                   && x.Homeroom.HomeroomTeachers.Any(y => y.IdBinusian == param.IdUser && y.IsAttendance
                                                   && (!string.IsNullOrEmpty(param.IdHomeroom) ? x.IdHomeroom == param.IdHomeroom : true)))
                                       .Include(x => x.AttendanceEntries).ThenInclude(x => x.AttendanceMappingAttendance).ThenInclude(x => x.Attendance)
                                       .ToListAsync();

            //data = data.Where(x => x.GeneratedScheduleStudent.IdStudent == "2070005566").ToList();

            var idSchool = _dbContext.Entity<MsAcademicYear>().Where(x => x.Id == param.IdAcademicYear).Select(x => x.IdSchool).FirstOrDefault();

            if (idSchool != null)
            {
                if (idSchool == "1")
                {
                    var data2 = new List<TrGeneratedScheduleLesson>();

                    foreach (var item in data.GroupBy(x => new { x.GeneratedScheduleStudent.IdStudent }).ToList())
                    {
                        var dataComparison = data.Where(x => x.GeneratedScheduleStudent.IdStudent == item.Key.IdStudent).ToList();
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
            }
            
            if (idSchool != null)
            {
                if (idSchool != "1")
                {
                    if (data.Count != 0)
                    {
                        if (data.Where(x => x.IdUser == param.IdUser).Count() > 0)
                        {
                            var selectTeacher = param.IdUser;
                            var dataComparison = data.Where(x => x.IdUser == selectTeacher).ToList();
                            if (dataComparison.Any(x => x.AttendanceEntries.Count == 1))
                            {
                                var dataAnyAttendance = data.OrderByDescending(x => x.AttendanceEntries.Count == 0 ? 0 : 1).ToList();
                                selectTeacher = dataAnyAttendance.First().IdUser;
                                data = data.Where(x => x.IdUser == selectTeacher).ToList();

                                var dataFinal = new List<TrGeneratedScheduleLesson>();
                                foreach (var idStudent in data.Select(x => x.GeneratedScheduleStudent.IdStudent).Distinct().ToList())
                                {
                                    if (data.Any(x => x.AttendanceEntries.Count != 0 && x.GeneratedScheduleStudent.IdStudent == idStudent))
                                    {
                                        dataFinal.Add(data.Where(x => x.AttendanceEntries.Count != 0 && x.GeneratedScheduleStudent.IdStudent == idStudent).FirstOrDefault());
                                    }
                                    else
                                    {
                                        dataFinal.Add(data.First(x => x.IdStudent == idStudent));
                                    }
                                }
                                data = dataFinal;
                            }
                            else
                            {
                                data = data.Where(x => x.IdUser == selectTeacher).ToList();
                            }
                        }
                        else
                        {
                            var dataAnyAttendance = data.OrderByDescending(x => x.AttendanceEntries.Count == 0 ? 0 : 1).ToList();
                            var selectTeacher = dataAnyAttendance.First().IdUser;

                            //var listTeacher = data.GroupBy(x => x.IdUser).Select(x => x.Key).ToList();

                            data = data.Where(x => selectTeacher.Contains(x.IdUser)).ToList();
                        }
                    }
                }
            }      

            if(checkStudentStatus != null)
            {
                data = data.Where(x => !checkStudentStatus.Select(z=>z.IdStudent).ToList().Contains(x.GeneratedScheduleStudent.IdStudent)).ToList();
            }

            var idLevels = data.Select(x => x.Homeroom.GradePathwayClassroom.GradePathway.Grade.IdLevel).Distinct().ToList();          

            var result = data.GroupBy(x => new { x.Homeroom, x.HomeroomName })
                             .Select(x => new HomeroomAttendanceResult
                             {
                                 Level = new CodeWithIdVm
                                 {
                                     Id = x.Key.Homeroom.GradePathwayClassroom.GradePathway.Grade.Level.Id,
                                     Code = x.Key.Homeroom.GradePathwayClassroom.GradePathway.Grade.Level.Code,
                                     Description = x.Key.Homeroom.GradePathwayClassroom.GradePathway.Grade.Level.Description,
                                 },
                                 Homeroom = new ItemValueVm
                                 {
                                     Id = x.Key.Homeroom.Id,
                                     Description = x.Key.HomeroomName
                                 },
                                 TotalStudent = x.GroupBy(x => x.GeneratedScheduleStudent.IdStudent).Count(),
                                 Pending = x.Key.Homeroom.GradePathwayClassroom.GradePathway.Grade.Level.MappingAttendances.Any(y => y.AbsentTerms == AbsentTerm.Day) ?
                                           x.Where(x => x.AttendanceEntries.Any(y => y.Status == AttendanceEntryStatus.Pending)).GroupBy(x => x.GeneratedScheduleStudent.IdStudent).Count() : x.Count(x => x.AttendanceEntries.Any(y => y.Status == AttendanceEntryStatus.Pending)),
                                 Submitted = x.Key.Homeroom.GradePathwayClassroom.GradePathway.Grade.Level.MappingAttendances.Any(y => y.AbsentTerms == AbsentTerm.Day) ?
                                             x.Where(x => x.AttendanceEntries.Any(y => y.Status == AttendanceEntryStatus.Submitted)).GroupBy(x => x.GeneratedScheduleStudent.IdStudent).Count() : x.Count(x => x.AttendanceEntries.Any(y => y.Status == AttendanceEntryStatus.Submitted)),
                                 Unsubmitted = x.Key.Homeroom.GradePathwayClassroom.GradePathway.Grade.Level.MappingAttendances.Any(y => y.AbsentTerms == AbsentTerm.Day) ?
                                               x.Where(x => !x.AttendanceEntries.Any()).GroupBy(x => x.GeneratedScheduleStudent.IdStudent).Count() : x.Count(x => !x.AttendanceEntries.Any()),
                                 UnexcusedAbsence = x.Key.Homeroom.GradePathwayClassroom.GradePathway.Grade.Level.MappingAttendances.Any(y => y.AbsentTerms == AbsentTerm.Day) ?
                                                    x.Where(x => x.AttendanceEntries.Any(y => y.AttendanceMappingAttendance.Attendance.AbsenceCategory == AbsenceCategory.Unexcused && y.Status == AttendanceEntryStatus.Submitted)).GroupBy(x => x.GeneratedScheduleStudent.IdStudent).Count() : x.Count(x => x.AttendanceEntries.Any(y => y.AttendanceMappingAttendance.Attendance.AbsenceCategory == AbsenceCategory.Unexcused && y.Status == AttendanceEntryStatus.Submitted)),
                                 LastSavedBy = x.Where(x => x.AttendanceEntries.Any()).Any() ? (x.Where(x => x.AttendanceEntries.Any()).OrderByDescending(x => x.AttendanceEntries.Max(y => y.DateIn)).First().AttendanceEntries.OrderByDescending(x => x.DateUp).Select(x => x.UserUp).First() ??
                                                                                                  x.Where(x => x.AttendanceEntries.Any()).OrderByDescending(x => x.AttendanceEntries.Max(y => y.DateIn)).First().AttendanceEntries.OrderByDescending(x => x.DateIn).Select(x => x.UserIn).First()) : null,
                                 LastSavedAt = x.Where(x => x.AttendanceEntries.Any()).Any() ? (x.Where(x => x.AttendanceEntries.Any()).OrderByDescending(x => x.AttendanceEntries.Max(y => y.DateIn)).First().AttendanceEntries.OrderByDescending(x => x.DateUp).Select(x => x.DateUp).First() ??
                                                                                                  x.Where(x => x.AttendanceEntries.Any()).OrderByDescending(x => x.AttendanceEntries.Max(y => y.DateIn)).First().AttendanceEntries.OrderByDescending(x => x.DateIn).Select(x => x.DateIn).First()) : null,
                             }).OrderBy(x => x.Homeroom.Description).ToList();

            var userIds = result.Select(x => x.LastSavedBy).Distinct().ToList();
            var users = await _dbContext.Entity<MsUser>()
                                        .Where(x => userIds.Contains(x.Id))
                                        .Select(x => new { x.Id, x.DisplayName })
                                        .ToListAsync();

            foreach (var item in result)
            {
                if (!string.IsNullOrEmpty(item.LastSavedBy))
                    item.LastSavedBy = users.First(x => x.Id == item.LastSavedBy).DisplayName;
            }

            return Request.CreateApiResult2(result as object);
        }
    }
}

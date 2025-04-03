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
    public class GetDetailAttendanceWorkhabitByStudentHandler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;

        public GetDetailAttendanceWorkhabitByStudentHandler(IAttendanceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetDetailAttendanceWorkhabitByStudentRequest>(
               nameof(GetDetailAttendanceWorkhabitByStudentRequest.StartDate),
               nameof(GetDetailAttendanceWorkhabitByStudentRequest.EndDate),
               nameof(GetDetailAttendanceWorkhabitByStudentRequest.IdStudent),
               nameof(GetDetailAttendanceWorkhabitByStudentRequest.IdMappingAttendanceWorkhabit));

            var data = await _dbContext.Entity<TrAttendanceEntryWorkhabit>()
                    .Include(x => x.MappingAttendanceWorkhabit)
                        .ThenInclude(x => x.Workhabit)
                    .Include(x => x.AttendanceEntry)
                        .ThenInclude(x => x.GeneratedScheduleLesson)
                            .ThenInclude(x => x.GeneratedScheduleStudent)
                                .ThenInclude(x => x.Student)
                                    .ThenInclude(x => x.StudentGrades)
                                        .ThenInclude(x => x.Grade)
                .Where(x => x.AttendanceEntry.GeneratedScheduleLesson.ScheduleDate.Date >= param.StartDate.Date)
                .Where(x => x.AttendanceEntry.GeneratedScheduleLesson.ScheduleDate.Date <= param.EndDate.Date)
                .Where(x => x.AttendanceEntry.GeneratedScheduleLesson.GeneratedScheduleStudent.IdStudent == param.IdStudent)
                .Where(x => x.IdMappingAttendanceWorkhabit == param.IdMappingAttendanceWorkhabit)
                .Where(x => x.AttendanceEntry.Status == Common.Model.Enums.AttendanceEntryStatus.Submitted)
                .OrderBy(x => x.AttendanceEntry.DateIn.Value)
                .Select(x => new GetDetailAttendanceWorkhabitByStudentResult
                {
                    Student = new CodeWithIdVm
                    {
                        Id = x.AttendanceEntry.GeneratedScheduleLesson.GeneratedScheduleStudent.Student.Id,
                        Code = x.AttendanceEntry.GeneratedScheduleLesson.GeneratedScheduleStudent.Student.FirstName,
                        Description = x.AttendanceEntry.GeneratedScheduleLesson.GeneratedScheduleStudent.Student.StudentGrades.FirstOrDefault().Grade.Description,
                    },
                    Workhabit = new CodeWithIdVm
                    {
                        Id = x.MappingAttendanceWorkhabit.Workhabit.Id,
                        Code = x.MappingAttendanceWorkhabit.Workhabit.Code,
                        Description = x.MappingAttendanceWorkhabit.Workhabit.Description
                    },
                    Date = x.AttendanceEntry.GeneratedScheduleLesson.ScheduleDate,
                    Session = x.AttendanceEntry.GeneratedScheduleLesson.SessionID,
                    Subject = x.AttendanceEntry.GeneratedScheduleLesson.SubjectName,
                    Teacher = x.AttendanceEntry.GeneratedScheduleLesson.TeacherName,
                    Comment = x.AttendanceEntry.Notes
                }).ToListAsync();
            return Request.CreateApiResult2(data as object);
        }
    }
}

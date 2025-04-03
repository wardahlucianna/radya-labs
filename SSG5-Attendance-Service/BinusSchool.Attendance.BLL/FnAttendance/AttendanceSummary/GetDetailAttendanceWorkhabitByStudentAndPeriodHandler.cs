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
    public class GetDetailAttendanceWorkhabitByStudentAndPeriodHandler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;

        public GetDetailAttendanceWorkhabitByStudentAndPeriodHandler(IAttendanceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetDetailAttendanceWorkhabitByStudentAndPeriodRequest>(
               nameof(GetDetailAttendanceWorkhabitByStudentAndPeriodRequest.Semester),
               nameof(GetDetailAttendanceWorkhabitByStudentAndPeriodRequest.IdStudent),
               nameof(GetDetailAttendanceWorkhabitByStudentAndPeriodRequest.IdMappingAttendanceWorkhabit));
            var data = await _dbContext.Entity<TrAttendanceEntryWorkhabit>()
                    .Include(x => x.MappingAttendanceWorkhabit)
                        .ThenInclude(x => x.Workhabit)
                    .Include(x => x.AttendanceEntry)
                        .ThenInclude(x => x.GeneratedScheduleLesson)
                            .ThenInclude(x => x.GeneratedScheduleStudent)
                                .ThenInclude(x => x.Student)
                                    .ThenInclude(x => x.StudentGrades)
                                        .ThenInclude(x => x.Grade)
                    .Include(x => x.AttendanceEntry)
                        .ThenInclude(x => x.GeneratedScheduleLesson)
                            .ThenInclude(x => x.Homeroom)
                .Where(x => x.AttendanceEntry.GeneratedScheduleLesson.Homeroom.Semester == param.Semester)
                .Where(x => x.AttendanceEntry.GeneratedScheduleLesson.GeneratedScheduleStudent.IdStudent == param.IdStudent)
                .Where(x => x.IdMappingAttendanceWorkhabit == param.IdMappingAttendanceWorkhabit)
                .Where(x => x.AttendanceEntry.Status == Common.Model.Enums.AttendanceEntryStatus.Submitted)
                .OrderBy(x => x.AttendanceEntry.DateIn.Value)
                .Select(x => new GetDetailAttendanceWorkhabitByStudentAndPeriodResult
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
                    Date = x.AttendanceEntry.DateIn.Value,
                    Session = x.AttendanceEntry.GeneratedScheduleLesson.SessionID,
                    Subject = x.AttendanceEntry.GeneratedScheduleLesson.SubjectName,
                    Teacher = x.AttendanceEntry.GeneratedScheduleLesson.TeacherName,
                    Comment = x.AttendanceEntry.Notes
                }).ToListAsync();
            return Request.CreateApiResult2(data as object);
        }
    }
}

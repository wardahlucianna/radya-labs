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
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Attendance.FnAttendance.AttendanceSummary
{
    public class GetDetailAttendanceWorkhabitByStudentAndPeriodTermDayHandler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;

        public GetDetailAttendanceWorkhabitByStudentAndPeriodTermDayHandler(IAttendanceDbContext dbContext)
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
                .Select(x => new
                {
                    idStudent = x.AttendanceEntry.GeneratedScheduleLesson.GeneratedScheduleStudent.Student.Id,
                    codeStudent = x.AttendanceEntry.GeneratedScheduleLesson.GeneratedScheduleStudent.Student.FirstName,
                    descStudent = x.AttendanceEntry.GeneratedScheduleLesson.GeneratedScheduleStudent.Student.StudentGrades.FirstOrDefault().Grade.Description,
                    idWorkHabit = x.MappingAttendanceWorkhabit.Workhabit.Id,
                    codeWorkhabit = x.MappingAttendanceWorkhabit.Workhabit.Code,
                    descWorkhabit = x.MappingAttendanceWorkhabit.Workhabit.Description,
                    idHomeRoom = x.AttendanceEntry.GeneratedScheduleLesson.IdHomeroom,
                    date = x.AttendanceEntry.GeneratedScheduleLesson.ScheduleDate,
                    dateIn = x.DateIn,
                    note = x.AttendanceEntry.Notes
                }).ToListAsync(CancellationToken);

            var newData = data.GroupBy(x => new { x.idStudent, x.codeStudent, x.descStudent, x.idWorkHabit, x.codeWorkhabit, x.descWorkhabit, x.date, x.dateIn, x.note, x.idHomeRoom });

            var homeroomIds = data.Select(x => x.idHomeRoom).Distinct();

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

            var result = new List<GetDetailAttendanceWorkhabitByStudentAndPeriodResult>();

            foreach (var item in newData)
            {
                var datas = new GetDetailAttendanceWorkhabitByStudentAndPeriodResult()
                {
                    Student = new CodeWithIdVm
                    {
                        Id = item.Key.idStudent,
                        Code = item.Key.codeStudent,
                        Description = item.Key.descStudent,
                    },
                    Workhabit = new CodeWithIdVm
                    {
                        Id = item.Key.idWorkHabit,
                        Code = item.Key.codeWorkhabit,
                        Description = item.Key.descWorkhabit
                    },
                    Date = item.Key.date,
                    Session = null,
                    Subject = null,
                    Teacher = homeroomTeachers.Where(y => y.IdHomeroom == item.Key.idHomeRoom).Select(y => y.Name).FirstOrDefault(),
                    Comment = item.Key.note
                };

                result.Add(datas);

            }

            return Request.CreateApiResult2(result as object);
        }
    }
}

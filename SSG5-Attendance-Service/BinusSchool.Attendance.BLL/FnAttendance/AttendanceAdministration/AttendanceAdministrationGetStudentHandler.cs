using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceAdministration;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using BinusSchool.Persistence.AttendanceDb.Entities.Student;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Attendance.FnAttendance.AttendanceAdministration
{
    public class AttendanceAdministrationGetStudentHandler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;

        public AttendanceAdministrationGetStudentHandler(IAttendanceDbContext dbContext, IMachineDateTime dateTime)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
        }


        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetAdministrationAttendanceStudentRequest>(nameof(GetAdministrationAttendanceStudentRequest.IdSchool));
            var columns = new[] { "idStudent", "name", "homeroom" };
            
            var dataStudents = _dbContext.Entity<MsHomeroomStudent>()
                .Include(x => x.Homeroom)
                    .ThenInclude(x => x.GradePathwayClassroom)
                        .ThenInclude(x => x.GradePathway)
                            .ThenInclude(x => x.Grade)
                .Include(x => x.HomeroomStudentEnrollments)
                .Include(x => x.Student)
                .Where(x => x.Student.GenerateScheduleStudents.Any(y => x.IdStudent == y.IdStudent))
                .AsQueryable();
            if (param.IdHomeroom?.Any() ?? false)
            {
                dataStudents = dataStudents.Where(x => param.IdHomeroom.Contains(x.IdHomeroom));
            }
            if (!string.IsNullOrWhiteSpace(param.IdGrade))
            {
                dataStudents = dataStudents.Where(x => x.Homeroom.GradePathwayClassroom.GradePathway.Grade.Id == param.IdGrade);
            }
            var data1 = await dataStudents.ToListAsync(CancellationToken);
            if (param.IdSubject?.Any() ?? false)
            {
                dataStudents = dataStudents.Where(x => x.HomeroomStudentEnrollments.Any(y => param.IdSubject.Contains(y.Lesson.IdSubject)));
            }
            var data2 = await dataStudents.ToListAsync(CancellationToken);
            if (param.IdStudent?.Any() ?? false)
            {
                dataStudents = dataStudents.Where(x => param.IdStudent.Contains(x.IdStudent));
            }

            var checkStudentStatus = await _dbContext.Entity<TrStudentStatus>().Select(x => new {x.IdStudent, x.StartDate, x.EndDate, x.IdStudentStatus, x.CurrentStatus, x.ActiveStatus, x.IdAcademicYear})
                .Where(x => x.ActiveStatus == false && x.CurrentStatus == "A" && (x.StartDate == _dateTime.ServerTime.Date || x.EndDate == _dateTime.ServerTime.Date
                    || (x.StartDate < _dateTime.ServerTime.Date
                        ? x.EndDate != null ? (x.EndDate > _dateTime.ServerTime.Date && x.EndDate < _dateTime.ServerTime.Date) || x.EndDate > _dateTime.ServerTime.Date : x.StartDate <= _dateTime.ServerTime.Date
                        : x.EndDate != null ? ((_dateTime.ServerTime.Date > x.StartDate && _dateTime.ServerTime.Date < x.EndDate) || _dateTime.ServerTime.Date > x.EndDate) : x.StartDate <= _dateTime.ServerTime.Date)))
                .ToListAsync();

            if(checkStudentStatus != null)
            {
                dataStudents = dataStudents.Where(x => !checkStudentStatus.Select(z=>z.IdStudent).ToList().Contains(x.IdStudent));
            }
            if (param.OrderType == OrderType.Asc)
            {
                switch (param.OrderBy)
                {
                    case "idStudent":
                        dataStudents = dataStudents.OrderBy(x => x.IdStudent);
                        break;
                    case "name":
                        dataStudents = dataStudents.OrderBy(x => x.Student.FirstName);
                        break;

                    case "homeroom":
                        dataStudents = dataStudents
                            .OrderBy(x => x.Homeroom.GradePathwayClassroom.GradePathway.Grade.Code.Length)
                                .ThenBy(x => x.Homeroom.GradePathwayClassroom.GradePathway.Grade.Code)
                                    .ThenBy(x => x.Homeroom.GradePathwayClassroom.Classroom.Code);
                        break;

                    default:
                        dataStudents = dataStudents.OrderBy(x => x.IdStudent);
                        break;
                }
            }
            else
            {
                switch (param.OrderBy)
                {
                    case "idStudent":
                        dataStudents = dataStudents.OrderByDescending(x => x.IdStudent);
                        break;
                    case "name":
                        dataStudents = dataStudents.OrderByDescending(x => x.Student.FirstName);
                        break;
                    case "homeroom":
                        dataStudents = dataStudents.OrderByDescending(x => x.Homeroom.GradePathwayClassroom.GradePathway.Grade.Code.Length)
                                .ThenByDescending(x => x.Homeroom.GradePathwayClassroom.GradePathway.Grade.Code)
                                    .ThenByDescending(x => x.Homeroom.GradePathwayClassroom.Classroom.Code);
                        break;

                    default:
                        dataStudents.OrderByDescending(x => x.IdStudent);
                        break;
                }
            }

            var data = await dataStudents
            .GroupBy(x => new
            {
                x.IdStudent,
                x.Student.FirstName,
                x.Student.MiddleName,
                x.Student.LastName,
                x.IdHomeroom,
                x.Homeroom.Semester,
                x.Homeroom.GradePathwayClassroom.GradePathway.IdGrade,
                GradeCode = x.Homeroom.GradePathwayClassroom.GradePathway.Grade.Code,
                ClassCode = x.Homeroom.GradePathwayClassroom.Classroom.Code
            })
            .Select(x => new
            {
                x.Key.IdStudent,
                x.Key.FirstName,
                x.Key.MiddleName,
                x.Key.LastName,
                x.Key.IdHomeroom,
                x.Key.Semester,
                x.Key.IdGrade,
                x.Key.GradeCode,
                x.Key.ClassCode
            })
            .SetPagination(param)
            .ToListAsync(CancellationToken);
            var res = data.Select(x => new GetAdministrationAttendanceStudentResult
            {
                IdStudent = x.IdStudent,
                Name = NameUtil.GenerateFullName(x.FirstName, x.MiddleName, x.LastName),
                HomeroomName = new CodeWithIdVm
                {
                    Id = x.IdHomeroom,
                    Code = x.GradeCode,
                    Description = string.Format("{0}{1} - {2}",
                    x.GradeCode,
                    x.ClassCode,
                    x.Semester) ?? null
                },
                IdGrade = x.IdGrade,
            }).ToList();

            return Request.CreateApiResult2(res as object, param.CreatePaginationProperty(res.Select(x => x.IdStudent).Count()).AddColumnProperty(columns));

        }
    }
}

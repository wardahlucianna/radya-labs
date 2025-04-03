using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Data.Model.Attendance.FnAttendance.EmergencyAttendanceV2;
using BinusSchool.Persistence.AttendanceDb.Entities;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Utils;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using BinusSchool.Data.Api.Student.FnStudent;
using BinusSchool.Persistence.AttendanceDb.Entities.User;

namespace BinusSchool.Attendance.FnAttendance.EmergencyAttendanceV2
{
    public class GetEmergencyReportDetailHandler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;

        public GetEmergencyReportDetailHandler(IAttendanceDbContext dbContext,
           IMachineDateTime dateTime)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetEmergencyReportDetailRequest>(nameof(GetEmergencyReportDetailRequest.idEmergencyReport));
            var columns = new[] { "idStudent", "studentname", "homeroom", "level" };

            var emergencyReport = await _dbContext.Entity<TrEmergencyReport>()
                                        .Where(a => a.Id == param.idEmergencyReport
                                        && a.SubmitStatus == true)
                                        .FirstOrDefaultAsync(CancellationToken);

            if (emergencyReport == null)
            {
                throw new BadRequestException("Emergency Report not found");
            }

            var periodActived = await _dbContext.Entity<MsPeriod>()
                                .Include(x => x.Grade)
                                    .ThenInclude(y => y.Level)
                                    .ThenInclude(y => y.AcademicYear)
                                .Where(a => a.Grade.Level.IdAcademicYear == emergencyReport.IdAcademicYear
                                && a.StartDate < _dateTime.ServerTime && _dateTime.ServerTime < a.EndDate)
                                .FirstOrDefaultAsync();

            if (periodActived == null)
            {
                periodActived = new MsPeriod() { Semester = 2 };

                //enhancement bug: untuk api ini untuk keperluan web, untuk cek history. di table transaksi tidak ada simpan semester, sedangkan di homeroomstudent mengikat semester,
                //hal ini dapat menyebabkan bug klo yang di cek beda semester, data student mungkin ada berubah.
            }

            var predicate = PredicateBuilder.True<MsHomeroomStudent>();

            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x
                    => EF.Functions.Like((!string.IsNullOrWhiteSpace(x.Student.FirstName) ? x.Student.FirstName + " " : "") + (!string.IsNullOrWhiteSpace(x.Student.MiddleName) ? x.Student.MiddleName + " " : "") + (x.Student.LastName), param.SearchPattern())
                    || EF.Functions.Like(x.IdStudent, param.SearchPattern())
                    || EF.Functions.Like((x.Homeroom.Grade.Code + "" + x.Homeroom.GradePathwayClassroom.Classroom.Code), param.SearchPattern())
                    );

            var StudentEmergencyAttendances = _dbContext.Entity<TrEmergencyAttendance>()
                 .Include(x => x.Student)
                 .Where(a => a.IdEmergencyStatus == (param.IdEmergencyStatus != null ? param.IdEmergencyStatus : a.IdEmergencyStatus)
                 && a.IdEmergencyReport == param.idEmergencyReport)
                 .AsQueryable();

            var getStudents = _dbContext.Entity<MsHomeroomStudent>()
                        .Include(x => x.Homeroom)
                            .ThenInclude(y => y.Grade)
                            .ThenInclude(y => y.Level)
                            .ThenInclude(y => y.AcademicYear)
                        .Include(x => x.Homeroom)
                            .ThenInclude(y => y.GradePathwayClassroom)
                            .ThenInclude(y => y.Classroom)
                        .Include(x => x.Student)
                            .ThenInclude(y => y.EmergencyAttendances)
                        .Where(a => StudentEmergencyAttendances.Select(b => b.IdStudent).Contains(a.IdStudent)
                         && a.Homeroom.Semester == (param.idHomeroom != null ? a.Homeroom.Semester : periodActived.Semester)
                         && a.Homeroom.Grade.Level.IdAcademicYear == emergencyReport.IdAcademicYear
                         && a.Homeroom.Grade.Level.Id == (param.idLevel != null ? param.idLevel : a.Homeroom.Grade.Level.Id)
                         && a.Homeroom.IdGrade == (param.idGrade != null ? param.idGrade : a.Homeroom.IdGrade)
                         && a.IdHomeroom == (param.idHomeroom != null ? param.idHomeroom : a.IdHomeroom)
                         )
                        .Where(predicate)
                        .OrderBy(a => a.Homeroom.Grade.Level.OrderNumber)
                        .ThenBy(b => b.Homeroom.Grade.Level.Code)
                        .ThenBy(b => b.Homeroom.Grade.OrderNumber)
                        .ThenBy(b => b.Homeroom.Grade.Code)
                        .ThenBy(b => b.Homeroom.GradePathwayClassroom.Classroom.Code)
                        .ThenBy(b => ((b.Student.FirstName != null ? b.Student.FirstName + " " : "") + (b.Student.MiddleName != null ? b.Student.MiddleName + " " : "") + b.Student.LastName))
                        .AsQueryable();

            if (getStudents.ToList().Count == 0)
            {
                List<GetEmergencyReportDetailResult> returnnull = new List<GetEmergencyReportDetailResult>();
                return Request.CreateApiResult2(returnnull as object, param.CreatePaginationProperty(0));
            }

            switch (param.OrderBy)
            {
                case "idStudent":
                    getStudents = param.OrderType == OrderType.Desc
                        ? getStudents.OrderByDescending(x => x.IdStudent)
                        : getStudents.OrderBy(x => x.IdStudent);
                    break;
                case "studentname":
                    getStudents = param.OrderType == OrderType.Desc
                        ? getStudents.OrderByDescending(x => ((x.Student.FirstName != null ? x.Student.FirstName + " " : "") + (x.Student.MiddleName != null ? x.Student.MiddleName + " " : "") + x.Student.LastName))
                        : getStudents.OrderBy(x => ((x.Student.FirstName != null ? x.Student.FirstName + " " : "") + (x.Student.MiddleName != null ? x.Student.MiddleName + " " : "") + x.Student.LastName));
                    break;
                case "homeroom":
                    getStudents = param.OrderType == OrderType.Desc
                        ? getStudents.OrderByDescending(x => (x.Homeroom.Grade.Code + "" + x.Homeroom.GradePathwayClassroom.Classroom.Code))
                        : getStudents.OrderBy(x => (x.Homeroom.Grade.Code + "" + x.Homeroom.GradePathwayClassroom.Classroom.Code));
                    break;
                case "level":
                    getStudents = param.OrderType == OrderType.Desc
                        ? getStudents.OrderByDescending(x => x.Homeroom.Grade.Level.Description)
                        : getStudents.OrderBy(x => x.Homeroom.Grade.Level.Description);
                    break;
                default:
                    getStudents = getStudents.OrderBy(a => a.Homeroom.Grade.OrderNumber).ThenBy(a => a.Homeroom.Grade.Code).ThenBy(a => a.Homeroom.GradePathwayClassroom.Classroom.Code).ThenBy(a => a.Student.FirstName + a.Student.LastName);
                    break;
            };


            List<GetEmergencyReportDetailResult_StudentVm> items = new List<GetEmergencyReportDetailResult_StudentVm>();
            if ((param.GetAll != null ? (bool)param.GetAll : false))
            {
                items = await getStudents
               .Select(a => new GetEmergencyReportDetailResult_StudentVm()
               {
                   IdHomeroom = a.IdHomeroom,
                   HomeroomName = a.Homeroom.Grade.Code + "" + a.Homeroom.GradePathwayClassroom.Classroom.Code,
                   IdLevel = a.Homeroom.Grade.IdLevel,
                   LevelName = a.Homeroom.Grade.Level.Description,
                   IdGrade = a.Homeroom.IdGrade,
                   GradeName = a.Homeroom.Grade.Description,
                   IdStudent = a.IdStudent,
                   StudentName = NameUtil.GenerateFullName(a.Student.FirstName, a.Student.MiddleName, a.Student.LastName),
               })
              .ToListAsync();
            }
            else
            {
                items = await getStudents
               .SetPagination(param)
               .Select(a => new GetEmergencyReportDetailResult_StudentVm()
               {
                   IdHomeroom = a.IdHomeroom,
                   HomeroomName = a.Homeroom.Grade.Code + "" + a.Homeroom.GradePathwayClassroom.Classroom.Code,
                   IdLevel = a.Homeroom.Grade.IdLevel,
                   LevelName = a.Homeroom.Grade.Level.Description,
                   IdGrade = a.Homeroom.IdGrade,
                   GradeName = a.Homeroom.Grade.Description,
                   IdStudent = a.IdStudent,
                   StudentName = NameUtil.GenerateFullName(a.Student.FirstName, a.Student.MiddleName, a.Student.LastName),
               })
              .ToListAsync();
            }


            var LtEmergencyStatus = await _dbContext.Entity<LtEmergencyStatus>()
                                   .OrderBy(a => a.Id)
                                   .Select(a => new
                                   {
                                       a.Id,
                                       a.EmergencyStatusName
                                   })
                                   .ToListAsync();

            var crossJoinResult = from student in items
                                  from emergencyStatus in LtEmergencyStatus
                                  select new
                                  {
                                      student.HomeroomName,
                                      student.LevelName,
                                      student.IdStudent,
                                      student.StudentName,
                                      idemergencyStatus = emergencyStatus.Id,
                                      emergencyStatusName = emergencyStatus.EmergencyStatusName
                                  };

            var crossJoinList = crossJoinResult.ToList();

            var getStudentEmergencyStatus = crossJoinList.GroupJoin(
                                    StudentEmergencyAttendances.ToList(),
                                    student => (student.IdStudent, student.idemergencyStatus),
                                    studEA => (studEA.IdStudent, studEA.IdEmergencyStatus),
                                    (student1, studEA1) => new { student = student1, studEA = studEA1 }
                                    )
                                    .SelectMany(EA => EA.studEA.DefaultIfEmpty(),
                                    (student1, studEA1) => new
                                    {
                                        IdStudent = student1.student.IdStudent,
                                        StudentName = student1.student.StudentName,
                                        HomeroomName = student1.student.HomeroomName,
                                        LevelName = student1.student.LevelName,
                                        idemergencyStatus = student1.student.idemergencyStatus,
                                        emergencyStatusName = student1.student.emergencyStatusName,
                                        isSelected = (studEA1 != null ? true : false),
                                        Description = (studEA1 != null ? studEA1?.Description ?? "" : ""),
                                        idEmergencyAttendance = (studEA1 != null ? studEA1?.Id ?? "" : ""),
                                        idUserIn = (studEA1 != null ? studEA1?.UserIn ?? "" : ""),
                                        idUserUp = (studEA1 != null ? studEA1?.UserUp?? "" : "")
                                       
                                    })
                                    .ToList();

            var getUser = await _dbContext.Entity<MsUser>()
                        .Where(a => getStudentEmergencyStatus.Select(b => b.idUserIn).Contains(a.Id)
                        || getStudentEmergencyStatus.Select(b => b.idUserUp).Contains(a.Id))
                        .ToListAsync();

            var GroupStudentEmergency = getStudentEmergencyStatus.GroupBy(a => new { a.IdStudent, a.StudentName, a.HomeroomName, a.LevelName })
                                                                .Select(a => new GetEmergencyReportDetailResult
                                                                {
                                                                    student = new ItemValueVm { Id = a.Key.IdStudent, Description = a.Key.StudentName },
                                                                    homeroomName = a.Key.HomeroomName,
                                                                    levelName = a.Key.LevelName,
                                                                    idEmergencyAttendance = a.Where(b => !string.IsNullOrWhiteSpace(b.idEmergencyAttendance)).FirstOrDefault().idEmergencyAttendance,
                                                                    emergencyStatusList = a.Select(a => new GetEmergencyReportDetailResult_EmergencyStatusVm()
                                                                    {
                                                                        idEmergencyStatus = a.idemergencyStatus,
                                                                        emergencyStatusName = a.emergencyStatusName,
                                                                        isSelected = a.isSelected,
                                                                        description = a.Description,
                                                                        markBy = (a.isSelected ? getUser.Where(b => b.Id == (a.idUserUp != "" ? a.idUserUp : a.idUserIn)).FirstOrDefault()?.DisplayName??"-" : ""),
                                                                        markStatus = (a.isSelected ?  (a.idUserUp != "" ? "U" : "I") : "")
                                                                    }).ToList()
                                                                }).ToList();

            var count = param.CanCountWithoutFetchDb(GroupStudentEmergency.Count)
                       ? GroupStudentEmergency.Count
                       : await getStudents.Select(x => x.IdStudent).CountAsync(CancellationToken);

            return Request.CreateApiResult2(GroupStudentEmergency as object, param.CreatePaginationProperty(count));
        }
    }
}



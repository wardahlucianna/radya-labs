using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Api.Student.FnStudent;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceAdministration;
using BinusSchool.Data.Model.Attendance.FnAttendance.EmergencyAttendanceV2;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using BinusSchool.Persistence.AttendanceDb.Entities.Student;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Org.BouncyCastle.Asn1.Pkcs;

namespace BinusSchool.Attendance.FnAttendance.EmergencyAttendanceV2
{
    public class GetStudentEmergencyAttendanceHandler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;

        public GetStudentEmergencyAttendanceHandler(IAttendanceDbContext dbContext,
            IMachineDateTime dateTime)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetStudentEmergencyAttendanceRequest>(nameof(GetStudentEmergencyAttendanceRequest.IdAcademicYear));
            var columns = new[] { "idStudent", "studentname", "homeroom", "level" };

            var predicate = PredicateBuilder.True<MsHomeroomStudent>();

            string GetDateTime = _dateTime.ServerTime.ToString("dd-MM-yyyy");
 
            var emergencyReport = await _dbContext.Entity<TrEmergencyReport>()
                                               .Where(a => a.IdAcademicYear == param.IdAcademicYear
                                               && a.SubmitStatus == false)
                                               .ToListAsync(CancellationToken);

            var periodActived = await _dbContext.Entity<MsPeriod>()
                                .Include(x => x.Grade)
                                    .ThenInclude(y => y.Level)
                                    .ThenInclude(y => y.AcademicYear)
                                .Where(a => a.Grade.Level.IdAcademicYear == param.IdAcademicYear
                                && a.IdGrade == (param.IdGrade != null ? param.IdGrade : a.IdGrade)
                                && a.Grade.IdLevel == (param.IdLevel != null ? param.IdLevel : a.Grade.IdLevel)
                                && a.StartDate < _dateTime.ServerTime && _dateTime.ServerTime < a.EndDate)
                                .FirstOrDefaultAsync();


            if (periodActived == null)
            {
                //hardcode, untuk jaga-jaga jika module dipakai ketika period tidak aktif
                periodActived = new MsPeriod() { Semester = 2 };
                //throw new BadRequestException("Period active not found");
            }

            //if(emergencyReport.Count == 0)
            //{
            //    throw new BadRequestException("Emergency Report not found");
            //}

            var emergencyReportActived = emergencyReport.Where(a => a.StartedDate.ToString("dd-MM-yyyy") == GetDateTime).OrderBy(a => a.StartedDate).FirstOrDefault();

            //if (emergencyReportActived == null)
            //{
            //    throw new BadRequestException("Emergency Report Actived not found");
            //}

            var StudentEmergency = await _dbContext.Entity<TrEmergencyAttendance>()
                                      .Include(x => x.EmergencyReport)
                                      .Where(a => a.IdEmergencyReport == (emergencyReportActived != null ? emergencyReportActived.Id : "-"))
                                      .ToListAsync(CancellationToken);


            var dataStudents = _dbContext.Entity<MsHomeroomStudent>()
                   //.Include(x => x.Homeroom)
                   //    .ThenInclude(x => x.Grade)
                   //    .ThenInclude(y => y.Level)
                   //    .ThenInclude(y => y.AcademicYear)
                   //.Include(y => y.Student)
                   //.Include(y => y.Homeroom)
                   //    .ThenInclude(y => y.GradePathwayClassroom)
                   //    .ThenInclude(y => y.Classroom)
               .Where(a => a.Homeroom.Grade.Level.AcademicYear.Id == (param.IdAcademicYear == null ? a.Homeroom.Grade.Level.IdAcademicYear : param.IdAcademicYear)
               && a.Homeroom.Grade.IdLevel == (param.IdLevel == null ? a.Homeroom.Grade.IdLevel : param.IdLevel)
               && a.Homeroom.IdGrade == (param.IdGrade == null ? a.Homeroom.IdGrade : param.IdGrade)
               && a.IdHomeroom == (param.IdHomeroom == null ? a.IdHomeroom : param.IdHomeroom)
               && a.Homeroom.Semester == (param.IdHomeroom == null && param.IdScheduleLesson == null ? periodActived.Semester : a.Homeroom.Semester)
               //&& (param.Status == "marked" ? StudentEmergency.Select(b => b.IdStudent).Contains(a.IdStudent) : !StudentEmergency.Select(b => b.IdStudent).Contains(a.IdStudent)) 
               )
               //.Where(predicate)
               .OrderBy(a => a.Homeroom.Grade.OrderNumber).ThenBy(a => a.Homeroom.GradePathwayClassroom.Classroom.Code).ThenBy(a => a.Student.FirstName + a.Student.LastName)
               .AsQueryable();

            if(dataStudents.ToList().Count == 0)
            {
                return Request.CreateApiResult2(new GetStudentEmergencyAttendanceResult() as object, param.CreatePaginationProperty(0));
            }



            if (!string.IsNullOrWhiteSpace(param.IdScheduleLesson))
            {
                var getStudentIdScheduleLesson = await _dbContext.Entity<MsHomeroomStudentEnrollment>()
                                                    .Include(x => x.HomeroomStudent)
                                                    .Include(x => x.Lesson)
                                                        .ThenInclude(y => y.ScheduleLesson)
                                                   .Where(a => a.Lesson.ScheduleLesson.Any(b => b.Id == param.IdScheduleLesson))
                                                   .Select(a => new { a.HomeroomStudent.IdStudent , a.HomeroomStudent.IdHomeroom })
                                                   .ToListAsync();

                dataStudents = dataStudents.Where(x => getStudentIdScheduleLesson.Select(a => a.IdStudent + "/" + a.IdHomeroom).Contains(x.Student.Id + "/"+ x.IdHomeroom));
            }


            var checkStudentStatus = await _dbContext.Entity<TrStudentStatus>()
                    .Select(x => new { x.IdStudent, x.StartDate, x.EndDate, x.IdStudentStatus, x.CurrentStatus, x.ActiveStatus, x.IdAcademicYear })
                    .Where(x => dataStudents.Select(a => a.IdStudent).Contains(x.IdStudent))
                    .Where(x => x.ActiveStatus == false
                            && x.CurrentStatus == "A"
                            && (x.StartDate == _dateTime.ServerTime.Date
                                || x.EndDate == _dateTime.ServerTime.Date
                                || (
                                        x.StartDate < _dateTime.ServerTime.Date ?
                                                    (
                                                        x.EndDate != null ?
                                                            ((x.EndDate > _dateTime.ServerTime.Date && x.EndDate < _dateTime.ServerTime.Date) || x.EndDate > _dateTime.ServerTime.Date)
                                                            : (x.StartDate <= _dateTime.ServerTime.Date)
                                                    )
                                                    : (x.EndDate != null ?
                                                            ((_dateTime.ServerTime.Date > x.StartDate && _dateTime.ServerTime.Date < x.EndDate) || _dateTime.ServerTime.Date > x.EndDate)
                                                            : x.StartDate <= _dateTime.ServerTime.Date)
                                    )
                                )
                          )
                    .ToListAsync();

            if (checkStudentStatus != null)
            {
                dataStudents = dataStudents.Where(x => !checkStudentStatus.Select(z => z.IdStudent).ToList().Contains(x.IdStudent));
            }

            #region
            //if (param.OrderType == OrderType.Desc)
            //{
            //    switch (param.OrderBy)
            //    {
            //        case "idStudent":
            //            dataStudents = dataStudents.OrderByDescending(x => x.IdStudent);
            //            break;
            //        case "name":
            //            dataStudents = dataStudents.OrderByDescending(x => x.StudentName);
            //            break;
            //        case "homeroom":
            //            dataStudents = dataStudents.OrderByDescending(x => x.HomeroomName);
            //            break;
            //        case "level":
            //            dataStudents = dataStudents.OrderByDescending(x => x.LevelName);
            //            break;
            //        default:
            //            dataStudents = dataStudents.OrderByDescending(x => x.HomeroomName).ThenByDescending(y => y.StudentName);
            //            break;
            //    }
            //}
            //else
            //{
            //    switch (param.OrderBy)
            //    {
            //        case "idStudent":
            //            dataStudents = dataStudents.OrderBy(x => x.IdStudent);
            //            break;
            //        case "name":
            //            dataStudents = dataStudents.OrderBy(x => x.StudentName);
            //            break;
            //        case "homeroom":
            //            dataStudents = dataStudents.OrderBy(x => x.HomeroomName);
            //            break;
            //        case "level":
            //            dataStudents = dataStudents.OrderBy(x => x.LevelName);
            //            break;
            //        default:
            //            dataStudents = dataStudents.OrderBy(x => x.HomeroomName).ThenBy(y => y.StudentName);
            //            break;
            //    }
            //}
            #endregion


            var LtEmergencyStatus = await _dbContext.Entity<LtEmergencyStatus>()
                                        .OrderBy(a => a.Id)
                                        .ToListAsync();

            GetStudentEmergencyAttendanceResult ReturnResult = new GetStudentEmergencyAttendanceResult();
            ReturnResult.IdEmergencyReportActived = (emergencyReportActived != null ? emergencyReportActived.Id : null);
            ReturnResult.totalStudent = dataStudents.Count();
            ReturnResult.totalStudentMarked = dataStudents.Where(a => StudentEmergency.Select(b => b.IdStudent).Contains(a.IdStudent)).Count();
            ReturnResult.totalStudentUnmarked = dataStudents.Count() - ReturnResult.totalStudentMarked;

            var datastudentFiltedStatus = dataStudents.Where(a => param.Status == "marked" ? StudentEmergency.Select(b => b.IdStudent).Contains(a.IdStudent) : !StudentEmergency.Select(b => b.IdStudent).Contains(a.IdStudent));
            
            if (param.IdEmergencyStatus != null && param.Status == "marked")
            {
                var getStudentByIdEmergencyStatus = StudentEmergency.Where(a => a.IdEmergencyStatus == param.IdEmergencyStatus).Select(b => b.IdStudent).ToList();
                datastudentFiltedStatus = datastudentFiltedStatus.Where(a => getStudentByIdEmergencyStatus.Contains(a.IdStudent));
            }

            if (!string.IsNullOrWhiteSpace(param.Search)) 
            {
                predicate = predicate.And(x
                        => EF.Functions.Like((!string.IsNullOrWhiteSpace(x.Student.FirstName) ? x.Student.FirstName + " " : "") + (!string.IsNullOrWhiteSpace(x.Student.MiddleName) ? x.Student.MiddleName + " " : "") + (x.Student.LastName), param.SearchPattern())
                         || EF.Functions.Like(x.IdStudent, param.SearchPattern())
                         || EF.Functions.Like((x.Homeroom.Grade.Code + "" + x.Homeroom.GradePathwayClassroom.Classroom.Code), param.SearchPattern())
                        );
                datastudentFiltedStatus = datastudentFiltedStatus.Where(predicate);
            }
                

            var items = await datastudentFiltedStatus
                  .SetPagination(param)
                  .Select(x => new GetStudentEmergencyAttendanceResult_StudentVm()
                  {
                      student = new ItemValueVm() { Id = x.IdStudent, Description = NameUtil.GenerateFullName(x.Student.FirstName, x.Student.MiddleName, x.Student.LastName) }, 
                      homeroom = new ItemValueVm() { Id = x.IdHomeroom, Description = x.Homeroom.Grade.Code + "" + x.Homeroom.GradePathwayClassroom.Classroom.Code },
                      level = new CodeWithIdVm() { Id = x.Homeroom.Grade.IdLevel, Description = x.Homeroom.Grade.Level.Description, Code = x.Homeroom.Grade.Level.Code },
                      emergencyStatusList = LtEmergencyStatus.Select(a => new GetStudentEmergencyAttendanceResult_EmergencyStatusVm()
                      {
                          idEmergencyStatus = a.Id,
                          emergencyStatusName = a.EmergencyStatusName
                      }).ToList()
                  })
            .ToListAsync();

            if(param.Status == "marked")
            {

              var studentemergenctList = StudentEmergency
                                    .SelectMany(a => LtEmergencyStatus, (a, b) => new 
                                    { 
                                        IdStudent = a.IdStudent,
                                        IdEmergencyAttendance = a.Id,
                                        IdEmergencyStatus = b.Id,
                                        EmergencyStatusName = b.EmergencyStatusName,
                                        Selected = (a.IdEmergencyStatus == b.Id),
                                        Desc = (a.IdEmergencyStatus == b.Id ? a.Description : "")
                                       
                                    })
                                    .GroupBy(a => new { a.IdStudent, a.IdEmergencyAttendance })
                                    .Select(a => new GetStudentEmergencyAttendance_Student2Vm()
                                    {
                                        IdStudent = a.Key.IdStudent,
                                        IdEmergencyAttendance = a.Key.IdEmergencyAttendance,
                                        emergencyStatusList = a.Select(b => new GetStudentEmergencyAttendanceResult_EmergencyStatusVm()
                                        {
                                            idEmergencyStatus = b.IdEmergencyStatus,
                                            emergencyStatusName = b.EmergencyStatusName,
                                            isSelected = b.Selected,
                                            description = b.Desc
                                        }).ToList()
                                    })
                                    .ToList();


                var results = items.GroupJoin(
                              studentemergenctList,
                              item => item.student.Id,
                              emergency => emergency.IdStudent,
                              (item1, emergency1) => new { item = item1, emegency = emergency1 }
                              ).SelectMany(m => m.emegency.DefaultIfEmpty(),
                              (item1, emergency1) => new GetStudentEmergencyAttendanceResult_StudentVm()
                              {
                                  idEmergencyAttendance = emergency1 != null ? emergency1.IdEmergencyAttendance : null,
                                  student = item1.item.student,
                                  homeroom = item1.item.homeroom,
                                  level = item1.item.level,
                                  emergencyStatusList = emergency1 != null ? emergency1.emergencyStatusList : null,
                              }
                              ).ToList();

                ReturnResult.studentList = results;
            }
            else
            {

                if (items.Count > 0)
                {
                    var StudentAbsentAndExecuse = await _dbContext.Entity<TrAttendanceEntryV2>()
                                          .Include(x => x.HomeroomStudent)
                                            .ThenInclude(y => y.Homeroom)
                                            .ThenInclude(y => y.Grade)
                                            .ThenInclude(y => y.Level)
                                            .ThenInclude(y => y.AcademicYear)
                                          .Include(x => x.AttendanceMappingAttendance)
                                            .ThenInclude(y => y.Attendance)
                                          .Include(x => x.ScheduleLesson)
                                          .Where(a =>
                                             a.HomeroomStudent.Homeroom.Grade.Level.AcademicYear.Id == (param.IdAcademicYear == null ? a.HomeroomStudent.Homeroom.Grade.Level.IdAcademicYear : param.IdAcademicYear)
                                             && a.HomeroomStudent.Homeroom.Grade.IdLevel == (param.IdLevel == null ? a.HomeroomStudent.Homeroom.Grade.IdLevel : param.IdLevel)
                                             && a.HomeroomStudent.Homeroom.IdGrade == (param.IdGrade == null ? a.HomeroomStudent.Homeroom.IdGrade : param.IdGrade)
                                             && a.HomeroomStudent.IdHomeroom == (param.IdHomeroom == null ? a.HomeroomStudent.IdHomeroom : param.IdHomeroom)
                                             //&& a.HomeroomStudent.Homeroom.Semester == (param.IdHomeroom == null && param.IdScheduleLesson == null ? periodActived.Semester : a.HomeroomStudent.Homeroom.Semester)
                                             && (
                                               (a.AttendanceMappingAttendance.Attendance.AttendanceCategory == AttendanceCategory.Present && a.AttendanceMappingAttendance.Attendance.AbsenceCategory == AbsenceCategory.Excused)
                                              || (a.AttendanceMappingAttendance.Attendance.AttendanceCategory == AttendanceCategory.Absent)
                                              )
                                             && items.Select(b => b.student.Id).Contains(a.HomeroomStudent.IdStudent)
                                             && a.ScheduleLesson.ScheduleDate.Date == _dateTime.ServerTime.Date
                                             )

                                          .Select(a => new
                                          {
                                              IdStudent = a.HomeroomStudent.IdStudent,
                                              AttendanceCategory = a.AttendanceMappingAttendance.Attendance.AttendanceCategory,
                                              AbsenceCategory = a.AttendanceMappingAttendance.Attendance.AbsenceCategory,
                                              AttStatus = a.Status,
                                              IsFromAttAdmin = a.IsFromAttendanceAdministration,
                                              Notes = a.Notes
                                          })
                                          .Distinct()
                                          .ToListAsync();

                    if (StudentAbsentAndExecuse.Count > 0)
                    {
                        foreach (var student in items)
                        {
                            var getstudentAbsentAndExecuse = StudentAbsentAndExecuse.Where(a => a.IdStudent == student.student.Id).FirstOrDefault();
                            if (getstudentAbsentAndExecuse != null)
                            {
                                if (getstudentAbsentAndExecuse.AttendanceCategory.Equals(AttendanceCategory.Present))
                                {
                                    //present & execuse
                                    student.emergencyStatusList = LtEmergencyStatus.Select(a => new GetStudentEmergencyAttendanceResult_EmergencyStatusVm()
                                    {
                                        idEmergencyStatus = a.Id,
                                        emergencyStatusName = a.EmergencyStatusName,
                                        isSelected = (a.EmergencyStatusName == "Other" ? true : false),
                                        description = (a.EmergencyStatusName == "Other" ? getstudentAbsentAndExecuse.Notes : null)
                                    }).ToList();
                                    student.isMarkFromAttendance = true;
                                }
                                else if (getstudentAbsentAndExecuse.AttendanceCategory.Equals(AttendanceCategory.Absent))
                                {
                                    //absent
                                    //present & execuse
                                    student.emergencyStatusList = LtEmergencyStatus.Select(a => new GetStudentEmergencyAttendanceResult_EmergencyStatusVm()
                                    {
                                        idEmergencyStatus = a.Id,
                                        emergencyStatusName = a.EmergencyStatusName,
                                        isSelected = (a.EmergencyStatusName == "Absent" ? true : false)
                                    }).ToList();
                                    student.isMarkFromAttendance = true;
                                }
                            }
                        }
                    }
                }

                ReturnResult.studentList = items.ToList();
            }


            var count = param.CanCountWithoutFetchDb(ReturnResult.studentList.Count)
                          ? ReturnResult.studentList.Count
                          : await datastudentFiltedStatus.Select(x => x.Id).CountAsync(CancellationToken);

            return Request.CreateApiResult2(ReturnResult as object, param.CreatePaginationProperty(count));

        }
    }
}

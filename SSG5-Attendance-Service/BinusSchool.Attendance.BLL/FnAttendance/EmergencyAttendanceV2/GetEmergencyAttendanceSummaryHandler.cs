using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.Attendance.FnAttendance.EmergencyAttendanceV2;
using BinusSchool.Data.Model.Scoring.FnScoring.PMBenchmark.PMBenchmarkEntry;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Common.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Common.Exceptions;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using BinusSchool.Persistence.AttendanceDb.Entities.Student;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Mvc;
using NPOI.SS.Formula.Functions;

namespace BinusSchool.Attendance.FnAttendance.EmergencyAttendanceV2
{
    public class GetEmergencyAttendanceSummaryHandler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;

        public GetEmergencyAttendanceSummaryHandler(IAttendanceDbContext dbContext,
            IMachineDateTime dateTime)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetEmergencyAttendanceSummaryRequest>(nameof(GetEmergencyAttendanceSummaryRequest.Date), nameof(GetEmergencyAttendanceSummaryRequest.IdAcademicYear));

            string GetDateTime = _dateTime.ServerTime.ToString("dd-MM-yyyy");

            var emergencyReport = await _dbContext.Entity<TrEmergencyReport>()
                                               .Where(a => a.StartedDate.Date == param.Date.Date
                                               && a.IdAcademicYear == param.IdAcademicYear
                                               && a.SubmitStatus == false)
                                               .OrderByDescending(a => a.DateIn)
                                               .FirstOrDefaultAsync(CancellationToken);

            //if (emergencyReport == null)
            //{
            //    throw new BadRequestException("Emergency Report Actived not found");
            //}

            var periodActived = await _dbContext.Entity<MsPeriod>()
                          .Include(x => x.Grade)
                              .ThenInclude(y => y.Level)
                              .ThenInclude(y => y.AcademicYear)
                          .Where(a => a.Grade.Level.IdAcademicYear == param.IdAcademicYear
                          && a.Grade.IdLevel == (param.IdLevel != null ? param.IdLevel : a.Grade.IdLevel)
                          && a.StartDate < _dateTime.ServerTime && _dateTime.ServerTime < a.EndDate)
                          .FirstOrDefaultAsync();

            if (periodActived == null)
            {
                //hardcode, untuk jaga-jaga jika module dipakai ketika period tidak aktif
                periodActived = new MsPeriod() { Semester = 2 };
                //throw new BadRequestException("Period active not found");
            }

            var StudentEmergency = await _dbContext.Entity<TrEmergencyAttendance>()
                                   .Include(x => x.EmergencyReport)
                                   .Where(a => a.IdEmergencyReport == (emergencyReport != null ? emergencyReport.Id : "-"))
                                   .ToListAsync(CancellationToken);

            var dataStudents = await _dbContext.Entity<MsHomeroomStudent>()
                .Include(x => x.Homeroom)
                    .ThenInclude(x => x.Grade)
                    .ThenInclude(y => y.Level)
                    .ThenInclude(y => y.AcademicYear)
                .Include(y => y.Student)
                .Include(y => y.Homeroom)
                    .ThenInclude(y => y.GradePathwayClassroom)
                    .ThenInclude(y => y.Classroom)
            .Where(a => a.Homeroom.Grade.Level.AcademicYear.Id == param.IdAcademicYear
            && a.Homeroom.Grade.IdLevel == (param.IdLevel == null ? a.Homeroom.Grade.IdLevel : param.IdLevel)
            && a.Homeroom.Semester == periodActived.Semester
            //&& (param.Status == "marked" ? StudentEmergency.Select(b => b.IdStudent).Contains(a.IdStudent) : !StudentEmergency.Select(b => b.IdStudent).Contains(a.IdStudent)) 
            )
            .Select(a => new
            {
                IdStudent = a.IdStudent,
                IdLevel = a.Homeroom.Grade.IdLevel,
                LevelCode = a.Homeroom.Grade.Level.Code,
                LevelName = a.Homeroom.Grade.Level.Description,
                LevelOrder = a.Homeroom.Grade.Level.OrderNumber,

            })
            .OrderBy(a => a.LevelOrder).ThenBy(a => a.LevelCode)
            .ToListAsync();

            if(dataStudents.Count == 0)
            {
                throw new BadRequestException("Student data not found");
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
                dataStudents = dataStudents.Where(x => !checkStudentStatus.Select(z => z.IdStudent).ToList().Contains(x.IdStudent)).ToList();
            }

            var results = dataStudents.GroupJoin(
                   StudentEmergency,
                   item => item.IdStudent,
                   emergency => emergency.IdStudent,
                   (item1, emergency1) => new { item = item1, emegency = emergency1 }
                   ).SelectMany(m => m.emegency.DefaultIfEmpty(),
                   (item1, emergency1) => new 
                   {
                       idEmergencyAttendance = emergency1 != null ? emergency1.Id : null,
                       IdStudent = item1.item.IdStudent,
                       IdLevel = item1.item.IdLevel,
                       LevelCode = item1.item.LevelCode,
                       LevelName = item1.item.LevelName
                   }
                   ).ToList();

            GetEmergencyAttendanceSummaryResult ReturnResult = new GetEmergencyAttendanceSummaryResult();

            List<GetEmergencyAttendanceSummary_LevelVm> SummaryLevel = results.GroupBy(a => new { a.IdLevel, a.LevelCode, a.LevelName })
                                                                            .Select(a => new GetEmergencyAttendanceSummary_LevelVm()
                                                                            {
                                                                                level = new CodeWithIdVm() { Id = a.Key.IdLevel, Code = a.Key.LevelCode, Description = a.Key.LevelName },
                                                                                TotalStudent = a.Count(),
                                                                                TotalMarked = a.Where(b => b.idEmergencyAttendance != null).ToList().Count(),
                                                                                TotalUnmarked = a.Where(b => b.idEmergencyAttendance == null).ToList().Count()
                                                                            }).ToList();
            ReturnResult.IdEmergencyReport = emergencyReport?.Id ?? "";
            ReturnResult.LevelSummarys = SummaryLevel;
            

            return Request.CreateApiResult2(ReturnResult as object);
        }
    }
}

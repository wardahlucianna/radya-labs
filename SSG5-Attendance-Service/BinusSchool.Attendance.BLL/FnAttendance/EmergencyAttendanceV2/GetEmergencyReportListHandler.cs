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
using BinusSchool.Data.Model.Attendance.FnAttendance.EmergencyAttendanceV2;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Azure.Documents.SystemFunctions;
using BinusSchool.Persistence.AttendanceDb.Entities.User;
using BinusSchool.Persistence.AttendanceDb.Entities.School;

namespace BinusSchool.Attendance.FnAttendance.EmergencyAttendanceV2
{
    public class GetEmergencyReportListHandler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;

        public GetEmergencyReportListHandler(IAttendanceDbContext dbContext,
            IMachineDateTime dateTime)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetEmergencyReportListRequest>();

            DateTime? startDatePeriod = null;
            DateTime? endDatePeriod = null;
            if (param.IdAcademicYear != null && param.semester != null)
            {
                var getMsPeriod = await _dbContext.Entity<MsPeriod>()
                               .Where(a => a.Grade.Level.IdAcademicYear == param.IdAcademicYear
                               && a.Semester == param.semester)
                               .ToListAsync();
                startDatePeriod = getMsPeriod.OrderBy(a => a.StartDate).FirstOrDefault().StartDate;
                endDatePeriod = getMsPeriod.OrderByDescending(a => a.EndDate).FirstOrDefault().EndDate;

                var tempStart = ((DateTime)startDatePeriod).ToString("dd-MMM-yyyy");
                var tempEnd = ((DateTime)endDatePeriod).ToString("dd-MMM-yyyy");
            }
           

            var EmergencyReportList = await _dbContext.Entity<TrEmergencyReport>()
                                        .Include(x => x.AcademicYear)
                                        .Where(a => a.SubmitStatus == true
                                        && a.IdAcademicYear == (param.IdAcademicYear != null ? param.IdAcademicYear : a.IdAcademicYear)
                                        && ((startDatePeriod != null ? startDatePeriod : ((DateTime)a.ReportedDate)) <= ((DateTime)a.ReportedDate))
                                        && ((endDatePeriod != null ? endDatePeriod : ((DateTime)a.ReportedDate)) >= ((DateTime)a.ReportedDate))
                                        && ((param.startDate != null ? ((DateTime)param.startDate) : ((DateTime)a.ReportedDate)) <= ((DateTime)a.ReportedDate))
                                        && ((param.endDate != null ? ((DateTime)param.endDate) : ((DateTime)a.ReportedDate)) >= ((DateTime)a.ReportedDate))
                                        )
                                        .Select(a => new 
                                        {
                                            academicYear =  new CodeWithIdVm() { Id = a.IdAcademicYear, Code = a.AcademicYear.Code, Description = a.AcademicYear.Description},
                                            idEmergencyReport = a.Id,
                                            startBy = a.StartedBy,
                                            startedDate = (a.StartedDate != null ? ((DateTime)a.StartedDate).ToString("dd-MMM-yyyy HH:mm") : "-"),
                                            reportedBy = a.ReportedBy,
                                            reportedDate = (a.ReportedDate != null ? ((DateTime)a.ReportedDate).ToString("dd-MMM-yyyy HH:mm") : "-"),
                                            RD = a.ReportedDate,
                                        })
                                        .OrderByDescending(a => a.RD)
                                        .ToListAsync(CancellationToken);

            var results2 = from report in EmergencyReportList
                          join user in _dbContext.Entity<MsUser>().Where(a => EmergencyReportList.Select(b => b.reportedBy).Contains(a.Id)).ToList()
                          on report.reportedBy equals user.Id into users
                          from user in users.DefaultIfEmpty()
                          join user2 in _dbContext.Entity<MsUser>().Where(a => EmergencyReportList.Select(b => b.startBy).Contains(a.Id)).ToList()
                          on report.startBy equals user2.Id into users2
                          from user2 in users2.DefaultIfEmpty()
                          select new GetEmergencyReportListResult
                          {
                              academicYear = report.academicYear,
                              idEmergencyReport = report.idEmergencyReport,
                              startedDate = report.startedDate,
                              startBy = user2 != null ? user2.DisplayName : report.startBy,
                              reportedDate = report.reportedDate,
                              reportedBy = user != null ? user.DisplayName : report.reportedBy
                          };

            var resultList = results2.ToList();
            
            return Request.CreateApiResult2(resultList as object);
        }
    }
}

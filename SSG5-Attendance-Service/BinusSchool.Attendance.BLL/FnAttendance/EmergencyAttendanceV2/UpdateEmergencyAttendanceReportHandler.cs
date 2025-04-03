using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Attendance.FnAttendance.EmergencyAttendanceV2.Validator;
using BinusSchool.Common.Abstractions;
using BinusSchool.Data.Model.Attendance.FnAttendance.EmergencyAttendanceV2;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Azure.Documents.SystemFunctions;
using BinusSchool.Persistence.AttendanceDb.Entities.Employee;
using BinusSchool.Persistence.AttendanceDb.Entities.User;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using BinusSchool.Common.Exceptions;
using BinusSchool.Persistence.AttendanceDb.Entities.Student;
using Microsoft.Azure.WebJobs;
using Newtonsoft.Json;

namespace BinusSchool.Attendance.FnAttendance.EmergencyAttendanceV2
{
    public class UpdateEmergencyAttendanceReportHandler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;

        public UpdateEmergencyAttendanceReportHandler(IAttendanceDbContext dbContext,
           IMachineDateTime dateTime)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<UpdateEmergencyAttendanceReportRequest, UpdateEmergencyAttendanceReportValidator>();

           
            UpdateEmergencyAttendanceReportResult returnresult = new UpdateEmergencyAttendanceReportResult();

            var emergencyReportActived = await _dbContext.Entity<TrEmergencyReport>()
                                              .Where(a => a.Id == body.IdEmergencyReport
                                              && a.StartedDate.Date == _dateTime.ServerTime.Date)
                                              .OrderByDescending(a => a.DateIn)
                                              .FirstOrDefaultAsync(CancellationToken);

            if (emergencyReportActived == null)
            {
                returnresult.status = false;
                returnresult.msg = "Emergency Report Actived not found";
                return Request.CreateApiResult2(returnresult as object);
            }
            else if(emergencyReportActived.SubmitStatus == true)
            {
                var getUser = await _dbContext.Entity<MsUser>().Where(a => a.Id == emergencyReportActived.ReportedBy).FirstOrDefaultAsync();
                returnresult.status = false;
                returnresult.msg = string.Format("Failed to submit. Emergency attendance already submitted by {0} on {1}", (getUser != null ? getUser.DisplayName : emergencyReportActived.ReportedBy), (emergencyReportActived.ReportedDate != null ? ((DateTime)emergencyReportActived.ReportedDate).ToString("dd-MM-yyyy HH:mm") : "-" ));
                return Request.CreateApiResult2(returnresult as object);
            }

            var periodActived = await _dbContext.Entity<MsPeriod>()
                                .Include(x => x.Grade)
                                    .ThenInclude(y => y.Level)
                                    .ThenInclude(y => y.AcademicYear)
                                .Where(a => a.Grade.Level.IdAcademicYear == emergencyReportActived.IdAcademicYear
                                && a.StartDate < _dateTime.ServerTime && _dateTime.ServerTime < a.EndDate)
                                .FirstOrDefaultAsync();

            if (periodActived == null)
            {
                periodActived = new MsPeriod() { Semester = 2 };
            }

            var dataStudents = await _dbContext.Entity<MsHomeroomStudent>()
                                      .Include(x => x.Homeroom)
                                          .ThenInclude(x => x.Grade)
                                          .ThenInclude(y => y.Level)
                                          .ThenInclude(y => y.AcademicYear)
                                      .Include(y => y.Student)
                                      .Include(y => y.Homeroom)
                                          .ThenInclude(y => y.GradePathwayClassroom)
                                          .ThenInclude(y => y.Classroom)
                                  .Where(a => a.Homeroom.Grade.Level.AcademicYear.Id == emergencyReportActived.IdAcademicYear
                                  && a.Homeroom.Semester == periodActived.Semester
                                  )
                                  .Select(a => new
                                  {
                                      IdStudent = a.IdStudent,
                                      IdLevel = a.Homeroom.Grade.IdLevel,
                                      LevelCode = a.Homeroom.Grade.Level.Code,
                                      LevelName = a.Homeroom.Grade.Level.Description,
                                      LevelOrder = a.Homeroom.Grade.Level.OrderNumber

                                  })
                                  .ToListAsync();

            if (dataStudents.Count == 0)
            {
                throw new BadRequestException("Student data not found");
            }

            var StudentEmergency = await _dbContext.Entity<TrEmergencyAttendance>()
                                  .Include(x => x.EmergencyReport)
                                  .Where(a => a.IdEmergencyReport == emergencyReportActived.Id)
                                  .ToListAsync(CancellationToken);

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

            if(results.Where(a => a.idEmergencyAttendance == null).Count() > 0)
            {
                returnresult.status = false;
                returnresult.msg = " Failed to submit. Please try again";
                
            }
            else
            {

                emergencyReportActived.ReportedBy = body.IdUserAction ?? AuthInfo.UserId;
                emergencyReportActived.ReportedDate = _dateTime.ServerTime;
                emergencyReportActived.SubmitStatus = true;
                _dbContext.Entity<TrEmergencyReport>().Update(emergencyReportActived);

                returnresult.status = true;
                returnresult.msg = " Attendance has been submitted successfully.";

                //add send email to queue
                /*
                Kondisi ketika berhasil submit, alertnya :
                -Emergency attendance submitted successfully. Parents of safe students have been notified by email

                */

                await _dbContext.SaveChangesAsync(CancellationToken);

                if (body.SendEmailStatus == true)
                {
                    var queueParam = new SendEmailEmergencyAttendanceQueue()
                    {
                        IdEmergencyReport = body.IdEmergencyReport
                    };


                    var newTrAttendanceQueue = new TrAttendanceQueue
                    {
                        Id = Guid.NewGuid().ToString(),
                        TriggerFrom = GetType().Name,
                        QueueName = "sendemail-emergency-attendance-queue",
                        Data = JsonConvert.SerializeObject(queueParam),
                        IsExecuted = false
                    };

                    _dbContext.Entity<TrAttendanceQueue>().Add(newTrAttendanceQueue);
                    await _dbContext.SaveChangesAsync(CancellationToken);

                    try
                    {
                        if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
                        {
                            var message = JsonConvert.SerializeObject(new QueueAttendanceRequest
                            {
                                IdAttendanceQueue = newTrAttendanceQueue.Id
                            });
                            collector.Add(message);
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message);
                    }

                     returnresult.msg = " Attendance has been submitted successfully. Parents of safe students have been notified by email";

                }

            }


            return Request.CreateApiResult2(returnresult as object);
        
        }
    }
}


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
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Data.Model.Attendance.FnAttendance.EmergencyAttendanceV2;
using BinusSchool.Persistence.AttendanceDb.Entities;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using Microsoft.CodeAnalysis.Operations;
using BinusSchool.Data.Api.Student.FnStudent;
using Microsoft.Azure.WebJobs;
using Newtonsoft.Json;

namespace BinusSchool.Attendance.FnAttendance.EmergencyAttendanceV2
{
    public class SaveStudentEmergencyAttendanceHandler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;

        public SaveStudentEmergencyAttendanceHandler(IAttendanceDbContext dbContext,
            IMachineDateTime dateTime)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {

            var body = await Request.ValidateBody<SaveStudentEmergencyAttendanceRequest, SaveStudentEmergencyAttendanceValidator>();

            int SuccessCount = 0;
            int FailedAlreadyExistCount = 0;

            SaveStudentEmergencyAttendanceResult returnresult = new SaveStudentEmergencyAttendanceResult();

            var emergencyReportActived = await _dbContext.Entity<TrEmergencyReport>()
                                                        .Where(a => a.IdAcademicYear == body.IdAcademicYear
                                                        && a.SubmitStatus == false
                                                        && a.StartedDate.Date == _dateTime.ServerTime.Date)
                                                        .OrderByDescending(a => a.DateIn)
                                                        .FirstOrDefaultAsync(CancellationToken);
            string IdEmergencyReportNew = "";
            string IdEmergencyReportUsed = "";
            if (emergencyReportActived == null)
            {
                IdEmergencyReportNew = Guid.NewGuid().ToString();
                IdEmergencyReportUsed = IdEmergencyReportNew;
                TrEmergencyReport addReport = new TrEmergencyReport();
                addReport.Id = IdEmergencyReportNew;
                addReport.IdAcademicYear = body.IdAcademicYear;
                addReport.StartedBy = body.RequestBy;
                addReport.StartedDate = _dateTime.ServerTime;
                addReport.SubmitStatus = false;
                _dbContext.Entity<TrEmergencyReport>().Add(addReport);

                await _dbContext.SaveChangesAsync(CancellationToken);
            }
            else
            {
                IdEmergencyReportUsed = emergencyReportActived.Id;
            }


            List<SaveStudentEmergencyAttendance_student> studentUpdate = body.studentList.Where(a => !string.IsNullOrEmpty(a.IdEmergencyAttendance)).ToList();
            List<SaveStudentEmergencyAttendance_student> studentAdd = body.studentList.Where(a => string.IsNullOrEmpty(a.IdEmergencyAttendance)).ToList();

            try
            {
               
                if (studentUpdate.Count > 0)
                {

                    var checkExisting = await _dbContext.Entity<TrEmergencyAttendance>()
                                  .Where(a => a.IdEmergencyReport == IdEmergencyReportUsed
                                  && studentUpdate.Select(b => b.IdEmergencyAttendance).Contains(a.Id))
                                  .ToListAsync();
                    List<TrEmergencyAttendance> dataUpdated = new List<TrEmergencyAttendance>();
                    List<TrEmergencyAttendance> dataDeleted = new List<TrEmergencyAttendance>();
                    List<HTrEmergencyAttendance> addHistoryList = new List<HTrEmergencyAttendance>();
                    foreach (var student in checkExisting)
                    {
                        var dataUpdate = studentUpdate.Where(a => a.IdEmergencyAttendance == student.Id).FirstOrDefault();
                        if (string.IsNullOrWhiteSpace(dataUpdate.IdEmergencyStatus))
                        {
                            student.IsActive = false;
                            dataDeleted.Add(student);

                            HTrEmergencyAttendance addHistory = new HTrEmergencyAttendance();
                            addHistory.IdEmergencyAttendanceHistory = student.Id;
                            addHistory.IdStudent = student.IdStudent;
                            addHistory.IdEmergencyReport = student.IdEmergencyReport;
                            addHistory.IdEmergencyStatus = student.IdEmergencyStatus;
                            addHistory.Description = student.Description;
                            addHistory.IsActive = student.IsActive;
                            addHistory.SendEmailStatus = student.SendEmailStatus;

                            addHistoryList.Add(addHistory);

                        }
                        else if (dataUpdate.IdEmergencyStatus != student.IdEmergencyStatus || student.Description != dataUpdate.Description)
                        {
                            student.IdEmergencyStatus = dataUpdate.IdEmergencyStatus;
                            student.Description = dataUpdate.Description;
                            student.SendEmailStatus = false;
                            dataUpdated.Add(student);
                        }
                    }
                    if(addHistoryList.Count() > 0)
                    {
                        _dbContext.Entity<HTrEmergencyAttendance>().AddRange(addHistoryList);
                    }

                    if (dataUpdated.Count() > 0)
                    {
                        _dbContext.Entity<TrEmergencyAttendance>().UpdateRange(dataUpdated);
                        SuccessCount += dataUpdated.Count();
                    }
                    if (dataDeleted.Count() > 0)
                    {
                        _dbContext.Entity<TrEmergencyAttendance>().RemoveRange(dataDeleted);
                        SuccessCount += dataDeleted.Count();
                    }

                }

                if (studentAdd.Count > 0)
                {

                    var addEmergencyAttendance = studentAdd
                        .Where(a => !string.IsNullOrWhiteSpace(a.IdEmergencyStatus))
                        .Select(a => new TrEmergencyAttendance
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdStudent = a.IdStudent,
                            IdEmergencyReport = IdEmergencyReportUsed,
                            IdEmergencyStatus = a.IdEmergencyStatus,
                            Description = a.Description,
                            SendEmailStatus = false
                        }).ToList();

                    var checkExisting = _dbContext.Entity<TrEmergencyAttendance>()
                                        .Where(a => a.IdEmergencyReport == IdEmergencyReportUsed
                                        && addEmergencyAttendance.Select(b => b.IdStudent).Contains(a.IdStudent))
                                        .ToList();

                    if (checkExisting.Any())
                    {
                        var dataDuplicate = addEmergencyAttendance.Where(a => checkExisting.Select(b => b.IdStudent + "/" + b.IdEmergencyStatus).Contains(a.IdStudent + "/" + a.IdEmergencyStatus)).ToList();
                        if (dataDuplicate.Any())
                        {
                            //ignore save & remove from addemergencyAttendance
                            addEmergencyAttendance = addEmergencyAttendance.Where(a => !dataDuplicate.Select(b => b.IdStudent).Contains(a.IdStudent)).ToList();
                            FailedAlreadyExistCount += dataDuplicate.Count();
                        }
                        //student saved & data tidak sama

                        if ((checkExisting.Count() - dataDuplicate.Count()) > 0)
                        {
                            var dataDelete = checkExisting.Where(a => !dataDuplicate.Select(b => b.IdStudent).Contains(a.IdStudent))
                                                          .ToList();

                            var moveHistory = dataDelete.Select(a => new HTrEmergencyAttendance()
                            {
                                IdEmergencyAttendanceHistory = a.Id,  
                                IdStudent = a.IdStudent,
                                IdEmergencyReport = a.IdEmergencyReport,
                                IdEmergencyStatus = a.IdEmergencyStatus,
                                Description = a.Description,
                                SendEmailStatus = a.SendEmailStatus
                            }).ToList();

                            _dbContext.Entity<HTrEmergencyAttendance>().AddRange(moveHistory);

                            _dbContext.Entity<TrEmergencyAttendance>().RemoveRange(dataDelete);
                            
                            await _dbContext.SaveChangesAsync(CancellationToken);
                        }
                        
                    }

                    if(addEmergencyAttendance.Count() > 0)
                    {
                        _dbContext.Entity<TrEmergencyAttendance>().AddRange(addEmergencyAttendance);
                        SuccessCount += addEmergencyAttendance.Count;
                    }
                    
                }

                await _dbContext.SaveChangesAsync(CancellationToken);

                if (SuccessCount > 0 && FailedAlreadyExistCount == 0)
                {
                    returnresult.status = true;
                    returnresult.msg = string.Format(" {0} student(s) has been saved successfully", SuccessCount.ToString());
                }
                else if (SuccessCount > 0 && FailedAlreadyExistCount > 0)
                {
                    returnresult.status = true;
                    returnresult.msg = string.Format(" {0} student(s) has been saved successfully. {1} student(s) couldn't be saved,  already updated by another user", SuccessCount.ToString(), FailedAlreadyExistCount.ToString());
                }
                else if (SuccessCount == 0 && FailedAlreadyExistCount > 0)
                {
                    returnresult.status = true;
                    returnresult.msg = string.Format(" {0} student(s) couldn't be saved, already updated by another user", FailedAlreadyExistCount.ToString());
                }
                else
                {
                    returnresult.status = true;
                    returnresult.msg = string.Format(" {0} student(s) has been saved successfully. {1} student(s) couldn't be saved", SuccessCount.ToString(), (body.studentList.Count - SuccessCount).ToString());
                }

            }
            catch (Exception ex)
            {
                returnresult.status = false;
                returnresult.msg = string.Format(" {0} student(s) failed to be saved. Please try again", body.studentList.Count().ToString());
            }
            return Request.CreateApiResult2(returnresult as object);
        }
    }
}

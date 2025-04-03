using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Policy;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using BinusSchool.Attendance.FnLongRun.Interfaces;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Api.Attendance.FnAttendance;
using BinusSchool.Data.Api.School.FnSchool;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummary;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummaryTerm;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceV2;
using BinusSchool.Data.Model.Attendance.FnAttendanceLongrun.Longrun;
using BinusSchool.Data.Model.Attendance.FnAttendanceLongrun.Longrun.AttendanceSummaryTerm;
using BinusSchool.Data.Model.Scheduling.FnAscTimetable.AscTimeTables.UploadXmlModel;
using BinusSchool.Data.Models.Binusian.BinusSchool.AttendanceLog;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using BinusSchool.Persistence.AttendanceDb.Entities.Student;
using BinusSchool.Persistence.AttendanceDb.Entities.User;
using BinusSchool.Persistence.AttendanceDb.Models;
using EllipticCurve.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NPOI.HPSF;
using NPOI.OpenXmlFormats.Spreadsheet;
using NPOI.SS.Formula.Functions;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.IO.Pem;

namespace BinusSchool.Attendance.FnLongRun.BlobTrigger.AttendanceSummaryTerm
{
    public class DailyAttendanceReportHandler
    {
#if DEBUG
        private const string _blobPath = "attendance-summary-daily-report/add/{name}.json";
#else
        private const string _blobPath = "attendance-summary-daily-report/add/{name}.json";
#endif

        private readonly IAttendanceDbContext _dbContext;
        private readonly IMachineDateTime _datetime;
        private readonly IStorageManager _storageManager;
        private readonly IAttendanceSummaryTerm _attendanceSummaryTermServices;
        private readonly IAttendanceV2 _attendanceV2;
        private readonly ISchool _schoolService;
        public DailyAttendanceReportHandler(IAttendanceDbContext dbContext, IMachineDateTime datetime, IStorageManager storageManager, IAttendanceSummaryTerm attendanceSummaryTermServices, ISchool schoolService, IAttendanceV2 attendanceV2)
        {
            _dbContext = dbContext;
            _storageManager = storageManager;
            _datetime = datetime;
            _attendanceSummaryTermServices = attendanceSummaryTermServices;
            _schoolService = schoolService;
            _attendanceV2 = attendanceV2;
        }

        [FunctionName(nameof(DailyAttendanceRecap))]
        public async Task DailyAttendanceRecap([BlobTrigger(_blobPath)] Stream blobStream,
            IDictionary<string, string> metadata,
            string name,
            CancellationToken cancellationToken)
        {
            var body = default(DailyAttendanceReportRequest);
            try
            {
                using var blobStreamReader = new StreamReader(blobStream);
                using var jsonReader = new JsonTextReader(blobStreamReader);

                while (await jsonReader.ReadAsync(cancellationToken))
                {
                    if (jsonReader.TokenType == JsonToken.StartObject)
                    {
                        body = new JsonSerializer().Deserialize<DailyAttendanceReportRequest>(jsonReader);
                        break;
                    }
                }

                var userRequest = await _dbContext.Entity<TrDailyAttendanceReportLog>()
                       .Where(e => e.UserIn == body.IdUser && e.IsProcess)
                       .FirstOrDefaultAsync(cancellationToken);

                if(userRequest!=null)
                    throw new Exception("Already Process");

                var start = await DailyAttendanceRecapLog(new DailyAttendanceRecapLogRequest
                {
                    Body = body,
                    CancellationToken = cancellationToken,
                    IsDone = false,
                    IsError = false
                });

                var excelRecap = await GenerateExcel(body, cancellationToken);

                #region save storage pdf
                var bytes = excelRecap;

                // save to storage
                var blobName = $"DailyStudentAbsenceReport-{Guid.NewGuid().ToString()}.xlsx";
                var blobContainer = await _storageManager.GetOrCreateBlobContainer("attendance-summary-daily-report", ct: cancellationToken);
                var blobClient = blobContainer.GetBlobClient(blobName);

                var blobResult = await blobClient.UploadAsync(new BinaryData(bytes), overwrite: true, cancellationToken);
                var rawBlobResult = blobResult.GetRawResponse();

                if (!(rawBlobResult.Status == StatusCodes.Status200OK || rawBlobResult.Status == StatusCodes.Status201Created))
                    throw new Exception(rawBlobResult.ReasonPhrase);

                // generate SAS uri with expire time in 10 minutes
                var sasUri = GenerateSasUri(blobClient);

                #endregion

                var finish = await DailyAttendanceRecapLog(new DailyAttendanceRecapLogRequest
                {
                    Body = body,
                    CancellationToken = cancellationToken,
                    IsDone = true,
                    IsError = false
                });

                #region Email 
                await _attendanceSummaryTermServices.SendAttendanceSumamryEmail(new SendAttendanceSumamryEmailRequest
                {
                    IdScenario = "EHN9",
                    IdUser = body.IdUser,
                    Link = sasUri.AbsoluteUri,
                    IdSchool = body.IdSchool
                });
                #endregion

            }
            catch (Exception ex)
            {
                var error = await DailyAttendanceRecapLog(new DailyAttendanceRecapLogRequest
                {
                    Body = body,
                    CancellationToken = cancellationToken,
                    IsDone = false,
                    IsError = true,
                    Message = ex.Message
                });

                #region Email 
                await _attendanceSummaryTermServices.SendAttendanceSumamryEmail(new SendAttendanceSumamryEmailRequest
                {
                    IdScenario = "EHN10",
                    IdUser = body.IdUser,
                    IdSchool = body.IdSchool
                });
                #endregion
            }
        }

        private Uri GenerateSasUri(BlobClient blobClient)
        {
            var wit = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            var dto = new DateTimeOffset(wit, TimeSpan.FromHours(DateTimeUtil.OffsetHour));

            // set expire time
            dto = dto.AddMonths(1);

            return blobClient.GenerateSasUri(Azure.Storage.Sas.BlobSasPermissions.Read, dto);
        }

        private async Task<string> DailyAttendanceRecapLog(DailyAttendanceRecapLogRequest data)
        {
            var dailyAttendanceRecapLog = await _dbContext.Entity<TrDailyAttendanceReportLog>()
                        .Where(e => e.UserIn == data.Body.IdUser && e.IsProcess)
                        .FirstOrDefaultAsync(data.CancellationToken);

            if (dailyAttendanceRecapLog == null)
            {
                TrDailyAttendanceReportLog newDailyAttendanceRecapLog = new TrDailyAttendanceReportLog
                {
                    Id = Guid.NewGuid().ToString(),
                    StartDate = _datetime.ServerTime,
                    UserIn = data.Body.IdUser,
                    IsProcess = true,
                };

                if (data.IsDone)
                {
                    newDailyAttendanceRecapLog.IsDone = data.IsDone;
                    newDailyAttendanceRecapLog.IsProcess = false;
                }

                if (data.IsError)
                {
                    newDailyAttendanceRecapLog.IsError = data.IsError;
                    newDailyAttendanceRecapLog.ErrorMessage = data.Message;
                    newDailyAttendanceRecapLog.IsProcess = false;
                }

                _dbContext.Entity<TrDailyAttendanceReportLog>().Add(newDailyAttendanceRecapLog);
            }
            else
            {
                dailyAttendanceRecapLog.EndDate = _datetime.ServerTime;
                dailyAttendanceRecapLog.IsProcess = true;

                if (data.IsDone)
                {
                    dailyAttendanceRecapLog.IsDone = data.IsDone;
                    dailyAttendanceRecapLog.IsProcess = false;
                }

                if (data.IsError)
                {
                    dailyAttendanceRecapLog.IsError = data.IsError;
                    dailyAttendanceRecapLog.ErrorMessage = data.Message;
                    dailyAttendanceRecapLog.IsProcess = false;
                }

                _dbContext.Entity<TrDailyAttendanceReportLog>().Update(dailyAttendanceRecapLog);
            }

            await _dbContext.SaveChangesAsync();
            return default;
        }

        public static List<GetHomeroom> GetMovingStudent(List<GetHomeroom> listStudentEnrollmentUnion, DateTime scheduleDate, string semester, string idLesson)
        {
            var listIdHomeroomStudentEnrollment = listStudentEnrollmentUnion.Where(e => e.IdLesson == idLesson).Select(e => e.IdHomeroomStudentEnrollment)
                                                    .Distinct().ToList();

            listStudentEnrollmentUnion = listStudentEnrollmentUnion.Where(e => listIdHomeroomStudentEnrollment.Contains(e.IdHomeroomStudentEnrollment)).ToList();

            var listStudentEnrollmentByDate = listStudentEnrollmentUnion
                                                .Where(e => e.EffectiveDate.Date <= scheduleDate.Date && e.Semester.ToString() == semester)
                                                .ToList();

            listIdHomeroomStudentEnrollment = listStudentEnrollmentByDate.Select(e => e.IdHomeroomStudentEnrollment).Distinct().ToList();

            var listStudentEnrollmentNew = new List<GetHomeroom>();
            foreach (var idHomeroomStudentEnrollment in listIdHomeroomStudentEnrollment)
            {
                var studentEnrollment = listStudentEnrollmentByDate
                                        .Where(e => e.IdHomeroomStudentEnrollment == idHomeroomStudentEnrollment)
                                        .LastOrDefault();

                if (studentEnrollment.IdLesson == idLesson && !studentEnrollment.IsDelete)
                {
                    listStudentEnrollmentNew.Add(studentEnrollment);
                }

            }

            return listStudentEnrollmentNew;
        }

        private async Task<List<GetPrasentResult>> GetPresent(PresentRequest data)
        {
            var getAttendanceUaPrasent = await _dbContext.Entity<TrAttendanceEntryV2>()
                                            .Include(e => e.Staff)
                                            .Include(e => e.ScheduleLesson).ThenInclude(e => e.Lesson)
                                            .Include(e => e.ScheduleLesson).ThenInclude(e => e.Session)
                                            .Include(e => e.HomeroomStudent).ThenInclude(e => e.Student)
                                            .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom).ThenInclude(e => e.Grade)
                                            .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom).ThenInclude(e => e.GradePathwayClassroom).ThenInclude(e => e.Classroom)
                                            .Include(e => e.AttendanceMappingAttendance).ThenInclude(e => e.Attendance)
                                            .Where(e => (e.AttendanceMappingAttendance.Attendance.Code == "PR" || e.AttendanceMappingAttendance.Attendance.AbsenceCategory == AbsenceCategory.Unexcused)
                                                && e.ScheduleLesson.IdAcademicYear == data.Body.IdAcademicYear && data.IdLessonUser.Contains(e.ScheduleLesson.IdLesson) && e.ScheduleLesson.ScheduleDate.Date == data.Date.Date)
                                            .GroupBy(e => new
                                            {
                                                IdStudent = e.HomeroomStudent.IdStudent,
                                                FirstName = e.HomeroomStudent.Student.FirstName,
                                                MiddleName = e.HomeroomStudent.Student.MiddleName,
                                                LastName = e.HomeroomStudent.Student.LastName,
                                                Class = e.HomeroomStudent.Homeroom.Grade.Code + e.HomeroomStudent.Homeroom.GradePathwayClassroom.Classroom.Code,
                                                Type = e.AttendanceMappingAttendance.Attendance.Code == "PR"
                                                            ? e.AttendanceMappingAttendance.Attendance.Description
                                                            : e.AttendanceMappingAttendance.Attendance.AbsenceCategory == AbsenceCategory.Unexcused
                                                                ? "UA"
                                                                : null,
                                                IdGrade = e.HomeroomStudent.Homeroom.IdGrade,
                                                IdHomeroomStudent = e.IdHomeroomStudent,
                                                Semester = e.ScheduleLesson.Lesson.Semester,
                                                ScheduleDate = e.ScheduleLesson.ScheduleDate,
                                                IdLesson = e.ScheduleLesson.IdLesson,
                                                ClassId = e.ScheduleLesson.ClassID,
                                                Session = e.ScheduleLesson.SessionID,
                                                Status = e.Status,
                                                CreatedBy = e.UserUp==null?e.UserIn:e.UserUp,
                                                IdLevel = e.ScheduleLesson.IdLevel,
                                                IdHomeroom = e.HomeroomStudent.IdHomeroom,
                                                Grade = e.HomeroomStudent.Homeroom.Grade.Code,
                                            })
                                            .Select(e => new //GetPrasentResult
                                            {
                                                IdStudent = e.Key.IdStudent,
                                                FirstName = e.Key.FirstName,
                                                MiddleName = e.Key.MiddleName,
                                                LastName = e.Key.LastName,
                                                Class = e.Key.Class,
                                                Type = e.Key.Type,
                                                IdGrade = e.Key.IdGrade,
                                                IdHomeroomStudent = e.Key.IdHomeroomStudent,
                                                Semester = e.Key.Semester,
                                                ScheduleDate = e.Key.ScheduleDate,
                                                IdLesson = e.Key.IdLesson,
                                                ClassId = e.Key.ClassId,
                                                Session = e.Key.Session,
                                                Status = e.Key.Status,
                                                CreatedBy = e.Key.CreatedBy,
                                                IdLevel = e.Key.IdLevel,
                                                IdHomeroom = e.Key.IdHomeroom,
                                                Grade = e.Key.Grade
                                            })
                                            .ToListAsync(data.CancellationToken);

            var listIdStudent = getAttendanceUaPrasent.Select(e => e.IdStudent).Distinct().ToList();
            var listIdGrade = getAttendanceUaPrasent.Select(e => e.IdGrade).Distinct().ToList();

            

            var listIdUser = getAttendanceUaPrasent.Select(e => e.CreatedBy).Distinct().ToList();
            var listUser = await _dbContext.Entity<MsUser>()
                    .Where(e=> listIdUser.Contains(e.Id))
                    .ToListAsync(data.CancellationToken);

            var getHomeroomTeacher = await _dbContext.Entity<MsHomeroomTeacher>()
                                            .Include(e=>e.Staff)
                                            .Where(e => e.Homeroom.IdAcademicYear == data.Body.IdAcademicYear && e.IsAttendance && e.TeacherPosition.LtPosition.Code==PositionConstant.ClassAdvisor)
                                            .ToListAsync(data.CancellationToken);

            var listStudentStatus = data.ListStudentStatus
                            .GroupBy(e => new RedisAttendanceSummaryStudentStatusResult
                            {
                                IdStudent = e.IdStudent,
                                StartDate = e.StartDate,
                                EndDate = Convert.ToDateTime(e.EndDate),
                            })
                            .Select(e => e.Key).ToList();

            List<GetPrasentResult> listPrasent = new List<GetPrasentResult>();
            foreach (var itemAttendanceEntry in getAttendanceUaPrasent)
            {
                var listStatusStudentByDate = listStudentStatus.Where(e => e.StartDate.Date <= itemAttendanceEntry.ScheduleDate.Date).Select(e => e.IdStudent).ToList();

                //moving student
                var listStudentEnrollmentUnionHomeroom = data.ListStudentEnrollmentUnion
                                                    .Where(e => e.IdHomeroomStudent == itemAttendanceEntry.IdHomeroomStudent 
                                                        && e.Semester == itemAttendanceEntry.Semester)
                                                    .OrderBy(e => e.IsFromMaster == true ? 0 : 1).ThenBy(e => e.EffectiveDate)
                                                    .ToList();

                var listStudentEnrollmentMoving = GetMovingStudent(listStudentEnrollmentUnionHomeroom, itemAttendanceEntry.ScheduleDate, itemAttendanceEntry.Semester.ToString(), itemAttendanceEntry.IdLesson);

                var studentEnrollmentMoving = listStudentEnrollmentMoving
                                              .Where(e => listStatusStudentByDate.Contains(e.IdStudent))
                                              .OrderBy(e => e.IsFromMaster == true ? 0 : 1).ThenBy(e => e.EffectiveDate)
                                              .ToList();

                if (studentEnrollmentMoving.Any() && itemAttendanceEntry.Type != null)
                {
                    var term = data.ListMappingAttendance.Where(e => e.IdLevel == itemAttendanceEntry.IdLevel).Select(e => e.AbsentTerms).FirstOrDefault();
                    GetPrasentResult newPrasent = new GetPrasentResult();

                    var user = listUser.Where(e => e.Id == itemAttendanceEntry.CreatedBy).FirstOrDefault();
                    if (user == null)
                        continue;

                    if (term== AbsentTerm.Session)
                    {
                        newPrasent = new GetPrasentResult
                        {
                            IdStudent = itemAttendanceEntry.IdStudent,
                            FirstName = itemAttendanceEntry.FirstName,
                            MiddleName = itemAttendanceEntry.MiddleName,
                            LastName = itemAttendanceEntry.LastName,
                            Class = itemAttendanceEntry.Class,
                            ClassId = itemAttendanceEntry.ClassId,
                            Session = Convert.ToInt32(itemAttendanceEntry.Session),
                            Status = itemAttendanceEntry.Status.GetDescription(),
                            Teacher = user.DisplayName,
                            Type = itemAttendanceEntry.Type,
                            Grade = itemAttendanceEntry.Grade,
                        };
                    }
                    else
                    {
                        newPrasent = new GetPrasentResult
                        {
                            IdStudent = itemAttendanceEntry.IdStudent,
                            FirstName = itemAttendanceEntry.FirstName,
                            MiddleName = itemAttendanceEntry.MiddleName,
                            LastName = itemAttendanceEntry.LastName,
                            Class = itemAttendanceEntry.Class,
                            Status = itemAttendanceEntry.Status.GetDescription(),
                            Teacher = user.DisplayName,
                            Type = itemAttendanceEntry.Type,
                            Grade = itemAttendanceEntry.Grade,
                        };
                    }

                    listPrasent.Add(newPrasent);
                }
            }

            var items = listPrasent
                        .GroupBy(e => new
                        {
                            IdStudent = e.IdStudent,
                            FirstName = e.FirstName,
                            MiddleName = e.MiddleName,
                            LastName = e.LastName,
                            Class = e.Class,
                            ClassId = e.ClassId,
                            Session = e.Session,
                            Status = e.Status,
                            Teacher = e.Teacher,
                            Type = e.Type,
                            Grade = e.Grade,
                        })
                        .Select(e => new GetPrasentResult
                        {
                            IdStudent = e.Key.IdStudent,
                            FirstName = e.Key.FirstName,
                            MiddleName = e.Key.MiddleName,
                            LastName = e.Key.LastName,
                            Class = e.Key.Class,
                            ClassId = e.Key.ClassId,
                            Session = e.Key.Session,
                            Status = e.Key.Status,
                            Teacher = e.Key.Teacher,
                            Type = e.Key.Type,
                            Grade = e.Key.Grade
                        }).ToList();

            return items;
        }

        private async Task<List<GetUnsubmitedResult>> GetUnsubmited(DailyAttendanceReportDefaultRequest data)
        {
            var listIdLevel = await _dbContext.Entity<MsLevel>()
                                        .Where(e => e.IdAcademicYear == data.Body.IdAcademicYear)
                                        .Select(e => e.Id)
                                        .ToListAsync(data.CancellationToken);

            List<GetUnsubmitedResult> listUnsubmited = new List<GetUnsubmitedResult>();
            foreach (var item in listIdLevel)
            {
                var paramUnsubmited = new GetAttendanceSummaryUnsubmitedRequest
                {
                    IdAcademicYear = data.Body.IdAcademicYear,
                    SelectedPosition = data.Body.SelectedPosition,
                    IdUser = data.Body.IdUser,
                    IdLevel = item,
                    GetAll = true,
                    StartDate = data.Date.Date,
                    EndDate = data.Date.Date
                };

                var getApiUnsubmited = await _attendanceSummaryTermServices.GetAttendanceSummaryUnsubmited(paramUnsubmited);
                if (getApiUnsubmited.IsSuccess)
                {
                    var listApiUnsubmited = getApiUnsubmited.Payload.ToList();
                    var GetUnsubmited = listApiUnsubmited.Select(e => new GetUnsubmitedResult
                    {
                        TeacherId = e.Teacher.Id,
                        TeacherName = e.Teacher.Description,
                        Class = e.Homeroom.Description,
                        Session = e.Session != null ? e.Session.SessionID : null
                    }).ToList();

                    listUnsubmited.AddRange(GetUnsubmited);
                }
            }

            var items = listUnsubmited
            .GroupBy(e => new
            {
                TeacherId = e.TeacherId,
                TeacherName = e.TeacherName,
                Class = e.Class,
                Session = e.Session
            })
            .Select(e => new GetUnsubmitedResult
            {
                TeacherId = e.Key.TeacherId,
                TeacherName = e.Key.TeacherName,
                Class = e.Key.Class,
                Session = e.Key.Session
            }).OrderBy(e => e.TeacherName).ThenBy(e => e.Class).ThenBy(e => e.Session).ToList();

            return items;
        }

        private async Task<List<GetTappingResult>> GetTapping(GetTappingRequest data)
        {
            var listIdStudentStatus = data.ListStudentStatus.Select(e => e.IdStudent).Distinct().ToList();
            var listStudent = await _dbContext.Entity<MsHomeroomStudentEnrollment>()
                                        .Include(e => e.HomeroomStudent).ThenInclude(e => e.Student)
                                        .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom).ThenInclude(e => e.Grade)
                                        .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom).ThenInclude(e => e.GradePathwayClassroom).ThenInclude(e => e.Classroom)
                                        .Where(e => e.HomeroomStudent.Homeroom.IdAcademicYear == data.Body.IdAcademicYear 
                                            && listIdStudentStatus.Contains(e.HomeroomStudent.IdStudent))
                                        .GroupBy(e => new 
                                        {
                                            IdStudent = e.HomeroomStudent.IdStudent,
                                            IdBinusian = e.HomeroomStudent.Student.IdBinusian,
                                            FirstName = e.HomeroomStudent.Student.FirstName,
                                            MiddleName = e.HomeroomStudent.Student.MiddleName,
                                            LastName = e.HomeroomStudent.Student.LastName,
                                            Class = e.HomeroomStudent.Homeroom.Grade.Code + e.HomeroomStudent.Homeroom.GradePathwayClassroom.Classroom.Code
                                        })
                                        .Select(e => new GetPrasentResult
                                        {
                                            IdStudent = e.Key.IdStudent,
                                            IdBinusian = e.Key.IdBinusian,
                                            FirstName = e.Key.FirstName,
                                            MiddleName = e.Key.MiddleName,
                                            LastName = e.Key.LastName,
                                            Class = e.Key.Class
                                        })
                                        .ToListAsync(data.CancellationToken);

            var listIdBinusian = listStudent.Select(e => e.IdBinusian).Distinct().ToList();

            var startHourtapping = new TimeSpan(0, 4, 0, 0);
            var EndHourtapping = new TimeSpan(0, 18, 0, 0);

            var AttendanceLogMachineRequest = new GetAttendanceLogRequest
            {
                IdSchool = data.Body.IdSchool,
                Year = data.Date.Date.Year,
                Month = data.Date.Date.Month,
                Day = data.Date.Date.Day,
                StartHour = startHourtapping.Hours,
                EndHour = EndHourtapping.Hours,
                StartMinutes = startHourtapping.Minutes,
                EndMinutes = EndHourtapping.Minutes,
                ListStudent = listIdBinusian
            };

            var getAttendanceTeppingLog = await _attendanceV2.GetAttendanceLogs(AttendanceLogMachineRequest);
            var listTapping = getAttendanceTeppingLog.IsSuccess ? getAttendanceTeppingLog.Payload.ToList() : null;

            List<GetTappingResult> listStudentTapping = new List<GetTappingResult>();
            foreach(var item in listTapping)
            {
                var StudentTapping = listStudent.Where(e => e.IdBinusian == item.BinusianID).FirstOrDefault();
                listStudentTapping.Add(new GetTappingResult
                {
                    IdStudent = StudentTapping.IdStudent,
                    IdBinusian = StudentTapping.IdBinusian,
                    FirstName = StudentTapping.FirstName,
                    MiddleName = StudentTapping.MiddleName,
                    LastName = StudentTapping.LastName,
                    Class = StudentTapping.Class,
                    DetectedDate = item.DetectedDate.TimeOfDay
                });
            }

            return listStudentTapping;
        }

        private async Task<List<GetPesentStudentResult>> GetPresentStudent(PresentRequest data)
        {
            var listIdLevel = data.ListMappingAttendance.Where(e => e.AbsentTerms == AbsentTerm.Session).Select(e => e.IdLevel).Distinct().ToList();

            var getScheduleLesson = await _dbContext.Entity<MsScheduleLesson>()
                                            .Include(e => e.AttendanceEntryV2s).ThenInclude(e=>e.AttendanceMappingAttendance).ThenInclude(e=>e.Attendance)
                                            .Include(e => e.AttendanceEntryV2s).ThenInclude(e=>e.HomeroomStudent).ThenInclude(e=>e.Student)
                                            .Include(e => e.AttendanceEntryV2s).ThenInclude(e=>e.HomeroomStudent).ThenInclude(e=>e.Homeroom).ThenInclude(e=>e.GradePathwayClassroom).ThenInclude(e => e.Classroom)
                                            .Include(e => e.AttendanceEntryV2s).ThenInclude(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom).ThenInclude(e => e.Grade)
                                            .IgnoreQueryFilters()
                                            .Where(e=>e.ScheduleDate==data.Date.Date 
                                                && e.IdAcademicYear==data.Body.IdAcademicYear 
                                                && listIdLevel.Contains(e.IdLevel)
                                                && ((e.IsActive && e.IsGenerated) || (!e.IsActive && e.IsDeleteFromEvent)))
                                            .Select(e => new
                                            { 
                                                ScheduleDate = e.ScheduleDate,
                                                IdLesson = e.IdLesson,
                                                Semester = e.Lesson.Semester,
                                                Session = e.SessionID,
                                                AttendanceEntry = e.AttendanceEntryV2s,
                                                IsDeleteFromEvent = e.IsDeleteFromEvent
                                            })
                                            .ToListAsync(data.CancellationToken);

            var listIdStudent = data.ListStudentEnrollmentUnion.Select(e => e.IdStudent).Distinct().ToList();

            var getEventStudent = await _dbContext.Entity<TrUserEvent>()
                                            .Where(e => (e.EventDetail.StartDate.Date <= data.Date.Date && e.EventDetail.EndDate.Date>= data.Date.Date)
                                                && e.EventDetail.Event.IdAcademicYear == data.Body.IdAcademicYear
                                                && listIdStudent.Contains(e.IdUser)
                                                && e.EventDetail.Event.EventIntendedFor.Any(f=>f.EventIntendedForAttendanceStudents.Any(g=>g.Type!= EventIntendedForAttendanceStudent.NoAtdClass))
                                                )
                                            .Select(e => e.IdUser)
                                            .ToListAsync(data.CancellationToken);

            var idDay = await _dbContext.Entity<LtDay>()
                                .Where(e => e.Description == data.Date.Date.DayOfWeek.ToString())
                                .Select(e => e.Id)
                                .FirstOrDefaultAsync(data.CancellationToken);

            var listSession = await _dbContext.Entity<MsSession>()
                                .Where(e => e.IdDay == idDay
                                    && e.GradePathway.Grade.Level.IdAcademicYear == data.Body.IdAcademicYear
                                    && listIdLevel.Contains(e.GradePathway.Grade.IdLevel))
                                .Select(e => e.SessionID)
                                .OrderBy(e=>e)
                                .Distinct()
                                .ToListAsync(data.CancellationToken);

            List<PesentStudent> listPresentStudent = new List<PesentStudent>();
            List<string> idStudentAttendance = new List<string>();
            var listStatusStudentByDate = data.ListStudentStatus.Select(e => e.IdStudent).Distinct().ToList();
            foreach (var itemSchedule in getScheduleLesson)
            {
                var listStudentEnrollmentMoving = GetMovingStudent(data.ListStudentEnrollmentUnion, itemSchedule.ScheduleDate, itemSchedule.Semester.ToString(), itemSchedule.IdLesson);

                var studentEnrollmentMoving = listStudentEnrollmentMoving
                                              .Where(e => listStatusStudentByDate.Contains(e.IdStudent))
                                              .OrderBy(e => e.IsFromMaster == true ? 0 : 1).ThenBy(e => e.EffectiveDate)
                                              .ToList();

                var listIdStudentMoving = studentEnrollmentMoving.Select(e => e.IdStudent).Distinct().ToList();

                var attendanceEntry = itemSchedule.AttendanceEntry.ToList();
                var attendanceEntryExsis = attendanceEntry.Where(e=> listIdStudentMoving.Contains(e.HomeroomStudent.IdStudent)).ToList();

                if (attendanceEntryExsis.Any())
                {
                    idStudentAttendance.Clear();
                    foreach (var itemAttendanceExsis in attendanceEntryExsis)
                    {
                        var status = "";
                        if (itemAttendanceExsis.AttendanceMappingAttendance.Attendance.Code == "PR" )
                            status = "V";
                        if (itemAttendanceExsis.AttendanceMappingAttendance.Attendance.Code == "LT")
                            status = itemAttendanceExsis.AttendanceMappingAttendance.Attendance.Code;
                        if (itemAttendanceExsis.AttendanceMappingAttendance.Attendance.AbsenceCategory== AbsenceCategory.Unexcused)
                            status = "UA";
                        if (itemAttendanceExsis.AttendanceMappingAttendance.Attendance.AbsenceCategory == AbsenceCategory.Excused)
                            status = "EA";
                        if (itemSchedule.IsDeleteFromEvent)
                            status = "CANCEL";

                        var isExsisEvent = getEventStudent.Where(e => e == itemAttendanceExsis.HomeroomStudent.IdStudent).Any();
                        if(isExsisEvent)
                            status = "EVENT";

                        if (string.IsNullOrEmpty(status))
                            continue;

                        listPresentStudent.Add(new PesentStudent
                        {
                            IdStudent = itemAttendanceExsis.HomeroomStudent.IdStudent,
                            StudentName = NameUtil.GenerateFullName(itemAttendanceExsis.HomeroomStudent.Student.FirstName, itemAttendanceExsis.HomeroomStudent.Student.MiddleName, itemAttendanceExsis.HomeroomStudent.Student.LastName),
                            Class = itemAttendanceExsis.HomeroomStudent.Homeroom.Grade.Code + itemAttendanceExsis.HomeroomStudent.Homeroom.GradePathwayClassroom.Classroom.Code,
                            Session = Convert.ToInt32(itemSchedule.Session),
                            Status = status,
                        });

                        idStudentAttendance.Add(itemAttendanceExsis.HomeroomStudent.IdStudent);
                    }
                }

                var attendanceEntryNoExsis = studentEnrollmentMoving.Where(e => !idStudentAttendance.Contains(e.IdStudent)).ToList();
                if (attendanceEntryNoExsis.Any())
                {
                    foreach (var itemAttendanceExsis in attendanceEntryNoExsis)
                    {
                        var status = "TBE";
                        if (itemSchedule.IsDeleteFromEvent)
                            status = "CANCEL";

                        var isExsisEvent = getEventStudent.Where(e => e == itemAttendanceExsis.IdStudent).Any();
                        if (isExsisEvent)
                            status = "EVENT";

                        listPresentStudent.Add(new PesentStudent
                        {
                            IdStudent = itemAttendanceExsis.IdStudent,
                            StudentName = NameUtil.GenerateFullName(itemAttendanceExsis.FirstName, itemAttendanceExsis.MiddleName, itemAttendanceExsis.LastName),
                            Class = itemAttendanceExsis.Grade.Code + itemAttendanceExsis.ClassroomCode,
                            Session = Convert.ToInt32(itemSchedule.Session),
                            Status = status,
                        });
                    }
                }
            }


            var listPresentStudentGrouping = listPresentStudent.OrderBy(e=>e.StudentName).OrderBy(e=>e.Session)
                .GroupBy(e => new
                {
                    e.IdStudent,
                    e.StudentName,
                    e.Class
                }).ToList();

            List<GetPesentStudentResult> listGetPrasentFix = new List<GetPesentStudentResult>();
            foreach (var item in listPresentStudentGrouping)
            {
                var attendanceEntry = item.ToList();
                var tapping = data.ListTapping.Where(e => e.IdStudent == item.Key.IdStudent).Select(e => e.DetectedDate).FirstOrDefault();
                List<GetPresentStudentSession> listGetPrasentSession = new List<GetPresentStudentSession>();
                foreach (var itemSession in listSession)
                {
                    var status = attendanceEntry.Where(e => e.Session == itemSession).Select(e=>e.Status).FirstOrDefault();

                    listGetPrasentSession.Add(new GetPresentStudentSession
                    {
                        Session = itemSession,
                        Status = status == null ? "X" : status
                    }) ;
                }

                listGetPrasentFix.Add(new GetPesentStudentResult
                {
                    IdStudent = item.Key.IdStudent,
                    StudentName = item.Key.StudentName,
                    Class = item.Key.Class,
                    Tapping = tapping,
                    ListSession = listGetPrasentSession,
                });
            }

            return listGetPrasentFix;
        }

        private async Task<byte[]> GenerateExcel(DailyAttendanceReportRequest Body, CancellationToken CancellationToken)
        {
            var workbook = new XSSFWorkbook();

#if DEBUG
            var date = Convert.ToDateTime("2023-11-09");
#else
            var date = _datetime.ServerTime.Date;
#endif

            #region style6
            var fontBold = workbook.CreateFont();
            fontBold.IsBold = true;

            var boldStyle = workbook.CreateCellStyle();
            boldStyle.Alignment = HorizontalAlignment.Center;
            boldStyle.VerticalAlignment = VerticalAlignment.Center;
            boldStyle.BorderBottom = BorderStyle.Thin;
            boldStyle.BorderLeft = BorderStyle.Thin;
            boldStyle.BorderRight = BorderStyle.Thin;
            boldStyle.BorderTop = BorderStyle.Thin;
            boldStyle.SetFont(fontBold);
            boldStyle.WrapText = true;

            //===================================================
            var fontBoldHeader = workbook.CreateFont();
            fontBoldHeader.IsBold = true;
            fontBoldHeader.FontHeightInPoints = 20;

            var boldStyleHeader = workbook.CreateCellStyle();
            boldStyleHeader.Alignment = HorizontalAlignment.Center;
            boldStyleHeader.VerticalAlignment = VerticalAlignment.Center;
            boldStyleHeader.WrapText = true;
            boldStyleHeader.SetFont(fontBoldHeader);

            //====================
            var fontNormalHeader = workbook.CreateFont();
            var normalStyleHeader = workbook.CreateCellStyle();
            normalStyleHeader.Alignment = HorizontalAlignment.Center;
            normalStyleHeader.VerticalAlignment = VerticalAlignment.Center;
            normalStyleHeader.SetFont(fontNormalHeader);


            //====================
            var fontNormalBody = workbook.CreateFont();
            var normalStyleBody = workbook.CreateCellStyle();
            normalStyleBody.BorderBottom = BorderStyle.Thin;
            normalStyleBody.BorderLeft = BorderStyle.Thin;
            normalStyleBody.BorderRight = BorderStyle.Thin;
            normalStyleBody.BorderTop = BorderStyle.Thin;
            normalStyleBody.Alignment = HorizontalAlignment.Center;
            normalStyleBody.VerticalAlignment = VerticalAlignment.Center;
            normalStyleBody.WrapText = true;
            normalStyleHeader.SetFont(fontNormalBody);

            //====================
            var boldStyleBody = workbook.CreateCellStyle();
            boldStyleBody.Alignment = HorizontalAlignment.Center;
            boldStyleBody.VerticalAlignment = VerticalAlignment.Center;
            boldStyleBody.WrapText = true;
            boldStyleBody.SetFont(fontBold);
            #endregion

            #region get Data
            var result = await _schoolService.GetSchoolDetail(Body.IdSchool);
            var schoolResult = result.IsSuccess ? result.Payload : throw new Exception(result.Message);

            byte[] logo = default;
            if (!string.IsNullOrEmpty(schoolResult.LogoUrl))
            {
                var blobNameLogo = schoolResult.LogoUrl;
                var blobContainerLogo = await _storageManager.GetOrCreateBlobContainer("school-logo", ct: CancellationToken);
                var blobClientLogo = blobContainerLogo.GetBlobClient(blobNameLogo);

                // generate SAS uri with expire time in 10 minutes
                var sasUri = GenerateSasUri(blobClientLogo);

                using var client = new HttpClient();
                logo = await client.GetByteArrayAsync(sasUri.AbsoluteUri);
            }

            var listStudentStatus = await _dbContext.Entity<TrStudentStatus>()
                                            .Where(e => e.IdAcademicYear == Body.IdAcademicYear
                                                && e.ActiveStatus
                                                && e.StartDate <= date.Date
                                                && (e.EndDate == null || e.EndDate >= date.Date))
                                            .ToListAsync(CancellationToken);

            var getStudentStatus = listStudentStatus
                                    .Select(e=>new RedisAttendanceSummaryStudentStatusResult
                                    {
                                        IdStudent = e.IdStudent,
                                        StartDate = e.StartDate,
                                        EndDate = e.EndDate==null?date:Convert.ToDateTime(e.EndDate)
                                    })
                                    .ToList();

            var getMappingAttendance = await _dbContext.Entity<MsMappingAttendance>()
                                            .Where(e => e.Level.IdAcademicYear == Body.IdAcademicYear)
                                            .Select(e=>new GetMappingAttendance
                                            {
                                                IdLevel = e.IdLevel,
                                                AbsentTerms = e.AbsentTerms
                                            })
                                            .ToListAsync(CancellationToken);

            var listPeriod = await _dbContext.Entity<MsPeriod>()
                                .Where(e => e.Grade.Level.IdAcademicYear == Body.IdAcademicYear)
                                .GroupBy(e => new
                                {
                                    Id = e.Id,
                                    IdGrade = e.IdGrade,
                                    StartDate = e.StartDate,
                                    EndDate = e.EndDate,
                                    Semester = e.Semester,
                                    IdLevel = e.Grade.IdLevel,
                                    AttendanceStartDate = e.AttendanceStartDate,
                                    AttendanceEndDate = e.AttendanceEndDate
                                })
                                .Select(e => e.Key)
                                .ToListAsync(CancellationToken);

            var listHomeroomStudentEnrollment = await _dbContext.Entity<MsHomeroomStudentEnrollment>()
                .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom).ThenInclude(e => e.Grade).ThenInclude(e => e.Level)
                .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom).ThenInclude(e => e.GradePathwayClassroom).ThenInclude(e => e.Classroom)
                .Include(e => e.Lesson)
                .Include(e => e.Subject)
                .Where(e => e.HomeroomStudent.Homeroom.Grade.Level.IdAcademicYear == Body.IdAcademicYear)
                .GroupBy(e => new GetHomeroom
                {
                    IdLesson = e.IdLesson,
                    IdHomeroomStudent = e.IdHomeroomStudent,
                    ClassId = e.Lesson.ClassIdGenerated,
                    IdStudent = e.HomeroomStudent.Student.Id,
                    FirstName = e.HomeroomStudent.Student.FirstName,
                    MiddleName = e.HomeroomStudent.Student.MiddleName,
                    LastName = e.HomeroomStudent.Student.LastName,
                    Homeroom = new ItemValueVm
                    {
                        Id = e.HomeroomStudent.IdHomeroom,
                        Description = e.HomeroomStudent.Homeroom.Grade.Code + e.HomeroomStudent.Homeroom.GradePathwayClassroom.Classroom.Code
                    },
                    IdClassroom = e.HomeroomStudent.Homeroom.GradePathwayClassroom.Classroom.Id,
                    ClassroomCode = e.HomeroomStudent.Homeroom.GradePathwayClassroom.Classroom.Code,
                    ClassroomName = e.HomeroomStudent.Homeroom.GradePathwayClassroom.Classroom.Description,
                    Grade = new CodeWithIdVm
                    {
                        Id = e.HomeroomStudent.Homeroom.IdGrade,
                        Code = e.HomeroomStudent.Homeroom.Grade.Code,
                        Description = e.HomeroomStudent.Homeroom.Grade.Description
                    },
                    Level = new CodeWithIdVm
                    {
                        Id = e.HomeroomStudent.Homeroom.Grade.Level.Id,
                        Code = e.HomeroomStudent.Homeroom.Grade.Level.Code,
                        Description = e.HomeroomStudent.Homeroom.Grade.Level.Description
                    },
                    Semester = e.HomeroomStudent.Homeroom.Semester,
                    IdSubject = e.Subject.Id,
                    SubjectCode = e.Subject.Code,
                    SubjectName = e.Subject.Description,
                    SubjectID = e.Subject.SubjectID,
                    IsFromMaster = true,
                    IsDelete = false,
                    IdHomeroomStudentEnrollment = e.Id
                })
                .Select(e => e.Key)
                .ToListAsync(CancellationToken);

            listHomeroomStudentEnrollment.ForEach(e => e.EffectiveDate = listPeriod.Where(f => f.IdGrade == e.Grade.Id).Select(f => f.AttendanceStartDate).Min());


            var listTrHomeroomStudentEnrollment = await _dbContext.Entity<TrHomeroomStudentEnrollment>()
                   .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom).ThenInclude(e => e.Grade).ThenInclude(e => e.Level)
                   .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom).ThenInclude(e => e.GradePathwayClassroom).ThenInclude(e => e.Classroom)
                   .Include(e => e.LessonNew)
                   .Include(e => e.SubjectNew)
                   .Where(e => e.HomeroomStudent.Homeroom.Grade.Level.IdAcademicYear == Body.IdAcademicYear)
                    .GroupBy(e => new GetHomeroom
                    {
                        IdLesson = e.IdLessonNew,
                        IdHomeroomStudent = e.IdHomeroomStudent,
                        ClassId = e.LessonNew.ClassIdGenerated,
                        IdStudent = e.HomeroomStudent.Student.Id,
                        FirstName = e.HomeroomStudent.Student.FirstName,
                        MiddleName = e.HomeroomStudent.Student.MiddleName,
                        LastName = e.HomeroomStudent.Student.LastName,
                        Homeroom = new ItemValueVm
                        {
                            Id = e.HomeroomStudent.IdHomeroom,
                            Description = e.HomeroomStudent.Homeroom.Grade.Code + e.HomeroomStudent.Homeroom.GradePathwayClassroom.Classroom.Code
                        },
                        IdClassroom = e.HomeroomStudent.Homeroom.GradePathwayClassroom.Classroom.Id,
                        ClassroomCode = e.HomeroomStudent.Homeroom.GradePathwayClassroom.Classroom.Code,
                        ClassroomName = e.HomeroomStudent.Homeroom.GradePathwayClassroom.Classroom.Description,
                        Grade = new CodeWithIdVm
                        {
                            Id = e.HomeroomStudent.Homeroom.IdGrade,
                            Code = e.HomeroomStudent.Homeroom.Grade.Code,
                            Description = e.HomeroomStudent.Homeroom.Grade.Description
                        },
                        Level = new CodeWithIdVm
                        {
                            Id = e.HomeroomStudent.Homeroom.Grade.Level.Id,
                            Code = e.HomeroomStudent.Homeroom.Grade.Level.Code,
                            Description = e.HomeroomStudent.Homeroom.Grade.Level.Description
                        },
                        Semester = e.HomeroomStudent.Homeroom.Semester,
                        IdSubject = e.SubjectNew.Id,
                        SubjectCode = e.SubjectNew.Code,
                        SubjectName = e.SubjectNew.Description,
                        SubjectID = e.SubjectNew.SubjectID,
                        IsDelete = e.IsDelete,
                        IsFromMaster = false,
                        EffectiveDate = e.StartDate,
                        IdHomeroomStudentEnrollment = e.IdHomeroomStudentEnrollment
                    })
                    .Select(e => e.Key)
                    .ToListAsync(CancellationToken);

            var listStudentEnrollmentUnion = listHomeroomStudentEnrollment.Union(listTrHomeroomStudentEnrollment)
                                                  .OrderBy(e => e.IsFromMaster == true ? 0 : 1).ThenBy(e => e.IsShowHistory == true ? 1 : 0).ThenBy(e => e.EffectiveDate).ThenBy(e => e.Datein)
                                                  .ToList();

            var paramGetIdLesson = new GetAttendanceSummaryUnsubmitedRequest
            {
                IdAcademicYear = Body.IdAcademicYear,
                SelectedPosition = Body.SelectedPosition,
                IdUser = Body.IdUser
            };

            var getApiLessonByPosition = await _attendanceSummaryTermServices.GetLessonByPosition(paramGetIdLesson);
            var getIdLessonUser = new List<string>();
            if (getApiLessonByPosition.IsSuccess)
            {
                getIdLessonUser = getApiLessonByPosition.Payload.ToList();
            }

            var _listPresent = await GetPresent(new PresentRequest
            {
                Body = Body,
                CancellationToken = CancellationToken,
                Date = date,
                ListMappingAttendance = getMappingAttendance,
                ListStudentStatus = getStudentStatus,
                IdLessonUser = getIdLessonUser,
                ListStudentEnrollmentUnion = listStudentEnrollmentUnion,
            });

            var listPresent = _listPresent
                        .GroupBy(e => new
                        {
                            IdStudent = e.IdStudent,
                            FirstName = e.FirstName,
                            MiddleName = e.MiddleName,
                            LastName = e.LastName,
                            Class = e.Class,
                            Type = e.Type,
                        })
                        .Select(e => new GetPrasentResult
                        {
                            IdStudent = e.Key.IdStudent,
                            FirstName = e.Key.FirstName,
                            MiddleName = e.Key.MiddleName,
                            LastName = e.Key.LastName,
                            Class = e.Key.Class,
                            Type = e.Key.Type,
                        }).ToList();

            var listTapping = await GetTapping(new GetTappingRequest
            {
                Body = Body,
                CancellationToken = CancellationToken,
                Date = date,
                ListStudentStatus = getStudentStatus
            });

            var _listPresentStudent = await GetPresentStudent(new PresentRequest
            {
                Body = Body,
                CancellationToken = CancellationToken,
                Date = date,
                ListMappingAttendance = getMappingAttendance,
                ListStudentStatus = getStudentStatus,
                IdLessonUser = getIdLessonUser,
                ListStudentEnrollmentUnion = listStudentEnrollmentUnion,
                ListTapping = listTapping,
            });


            #endregion
            var paramSheet1 = new SheetRequest
            {
                Workbook = workbook,
                BoldStyleHeader = boldStyleHeader,
                NormalStyleHeader = normalStyleHeader,
                BoldStyle = boldStyle,
                NormalStyleBody = normalStyleBody,
                BoldStyleBody = boldStyleBody,
                CancellationToken = CancellationToken,
                Body = Body,
                ListIdLessonUser = getIdLessonUser,
                ListPresent = listPresent,
                Logo = logo,
                ListStudentStatus = getStudentStatus,
                Date = date,
                ListTapping = listTapping,
                ListMappingAttendance = getMappingAttendance,
                ListStudentEnrollmentUnion = listStudentEnrollmentUnion
            };
            await GetSheet1(paramSheet1);

            var paramSheet2 = new SheetRequest
            {
                Workbook = workbook,
                BoldStyleHeader = boldStyleHeader,
                NormalStyleHeader = normalStyleHeader,
                BoldStyle = boldStyle,
                NormalStyleBody = normalStyleBody,
                BoldStyleBody = boldStyleBody,
                CancellationToken = CancellationToken,
                Body = Body,
                ListIdLessonUser = getIdLessonUser,
                ListPresent = _listPresent,
                Logo = logo,
                Date = date,
                ListTapping = listTapping,
                ListMappingAttendance = getMappingAttendance,
                ListStudentEnrollmentUnion = listStudentEnrollmentUnion,
                ListStudentStatus = getStudentStatus,
            };

            await GetSheet2(paramSheet2);
            await GetSheet3(paramSheet2);
            await GetSheet4(paramSheet2);

            using var ms = new MemoryStream();
            ms.Position = 0;
            workbook.Write(ms);

            return ms.ToArray();
        }

        private async Task<string> GetSheet1(SheetRequest data)
        {
            var sheet = data.Workbook.CreateSheet("UA Student");

            #region get Data
            var listUnsubmited = await GetUnsubmited(new DailyAttendanceReportDefaultRequest
            {
                Body = data.Body,
                CancellationToken = data.CancellationToken,
                Date = data.Date,
            });
            #endregion

            #region Add Logo
            if (data.Logo != null)
            {
                byte[] dataImg = data.Logo;
                int pictureIndex = data.Workbook.AddPicture(dataImg, PictureType.PNG);
                ICreationHelper helper = data.Workbook.GetCreationHelper();
                IDrawing drawing = sheet.CreateDrawingPatriarch();
                IClientAnchor anchor = helper.CreateClientAnchor();
                anchor.Col1 = 0;//0 index based column
                anchor.Row1 = 0;//0 index based row
                IPicture picture = drawing.CreatePicture(anchor, pictureIndex);
                picture.Resize(1.8, 2.5);
            }
            #endregion

            #region Row 1
            var rowIndex = 0;
            var rowHeader = sheet.CreateRow(rowIndex);

            #region List Of UA Student
            int startColumn = 2;
            var cellNo = rowHeader.CreateCell(startColumn);
            var merge = new NPOI.SS.Util.CellRangeAddress(rowIndex, rowIndex, startColumn, startColumn + 3);
            cellNo.SetCellValue("List of UA Student");
            cellNo.CellStyle = data.BoldStyleHeader;
            sheet.AddMergedRegion(merge);
            #endregion

            #region List Teachers who have not filled Attendance
            startColumn = 8;
            cellNo = rowHeader.CreateCell(startColumn);
            merge = new NPOI.SS.Util.CellRangeAddress(rowIndex, rowIndex, startColumn, startColumn + 4);
            cellNo.SetCellValue("Teachers who have");
            cellNo.CellStyle = data.BoldStyleHeader;
            sheet.AddMergedRegion(merge);
            #endregion

            #region List Teachers who have not filled Attendance
            startColumn = 14;
            cellNo = rowHeader.CreateCell(startColumn);
            merge = new NPOI.SS.Util.CellRangeAddress(rowIndex, rowIndex, startColumn, startColumn + 3);
            cellNo.SetCellValue("Students who did not Tap In");
            cellNo.CellStyle = data.BoldStyleHeader;
            sheet.AddMergedRegion(merge);
            #endregion
            rowIndex++;
            #endregion

            #region Row 2
            rowHeader = sheet.CreateRow(rowIndex);

            #region List Of UA Student
            startColumn = 2;
            cellNo = rowHeader.CreateCell(startColumn);
            merge = new NPOI.SS.Util.CellRangeAddress(rowIndex, rowIndex, startColumn, startColumn + 3);
            cellNo.SetCellValue($"as at {data.Date.ToString("dd MMMM yyyy HH:mm")}");
            sheet.AddMergedRegion(merge);
            cellNo.CellStyle = data.NormalStyleHeader;
            #endregion

            #region List Teachers who have not filled Attendance
            startColumn = 8;
            cellNo = rowHeader.CreateCell(startColumn);
            merge = new NPOI.SS.Util.CellRangeAddress(rowIndex, rowIndex, startColumn, startColumn + 4);
            cellNo.SetCellValue("not filled Attendance");
            sheet.AddMergedRegion(merge);
            cellNo.CellStyle = data.BoldStyleHeader;
            #endregion
            #endregion

            #region Header Table
            rowIndex = 4;
            rowHeader = sheet.CreateRow(rowIndex);

            #region List Of UA Student
            List<string> header = new List<string>();
            header.Add("No");
            header.Add("Student ID");
            header.Add("Student Name");
            header.Add("Class");

            startColumn = 2;
            foreach (var itemHeader in header)
            {
                cellNo = rowHeader.CreateCell(startColumn);
                cellNo.SetCellValue(itemHeader);
                cellNo.CellStyle = data.BoldStyle;
                sheet.AutoSizeColumn(startColumn, true);
                startColumn++;
            }
            rowIndex++;
            #endregion

            #region  List Teachers who have not filled Attendance
            header = new List<string>();
            header.Add("No");
            header.Add("Teacher ID");
            header.Add("Teacher Name");
            header.Add("Class");
            header.Add("Session");

            startColumn = 8;
            foreach (var itemHeader in header)
            {
                cellNo = rowHeader.CreateCell(startColumn);
                cellNo.SetCellValue(itemHeader);
                cellNo.CellStyle = data.BoldStyle;
                sheet.AutoSizeColumn(startColumn, true);
                startColumn++;
            }
            rowIndex++;
            #endregion

            #region  List Students who did not Tap In
            header = new List<string>();
            header.Add("No");
            header.Add("Student ID");
            header.Add("Student Name");
            header.Add("Class");

            startColumn = 14;
            foreach (var itemHeader in header)
            {
                cellNo = rowHeader.CreateCell(startColumn);
                cellNo.SetCellValue(itemHeader);
                cellNo.CellStyle = data.BoldStyle;
                sheet.AutoSizeColumn(startColumn, true);
                startColumn++;
            }
            rowIndex++;
            #endregion
            #endregion

            #region Body Table
            #region List Of UA Student
            var listUa = data.ListPresent.Where(e => e.Type == "UA").OrderBy(e => e.FirstName).ThenBy(e => e.MiddleName).ThenBy(e => e.LastName).ToList();

            List<int> listCount = new List<int> { listUa.Count(), listUnsubmited.Count(), data.ListTapping.Count() };
            var count = listCount.Max();

            rowIndex = 5;
            for (var i = 0; i < count; i++)
            {
                rowHeader = sheet.CreateRow(rowIndex);

                if (i < listUa.Count())
                {
                    var itemUa = listUa[i];

                    startColumn = 2;

                    cellNo = rowHeader.CreateCell(startColumn);
                    cellNo.SetCellValue(i + 1);
                    cellNo.CellStyle = data.NormalStyleBody;
                    sheet.AutoSizeColumn(startColumn, true);
                    startColumn++;

                    cellNo = rowHeader.CreateCell(startColumn);
                    cellNo.SetCellValue(itemUa.IdStudent);
                    cellNo.CellStyle = data.NormalStyleBody;
                    sheet.AutoSizeColumn(startColumn, true);
                    startColumn++;

                    cellNo = rowHeader.CreateCell(startColumn);
                    cellNo.SetCellValue(NameUtil.GenerateFullName(itemUa.FirstName, itemUa.MiddleName, itemUa.LastName));
                    cellNo.CellStyle = data.NormalStyleBody;
                    sheet.AutoSizeColumn(startColumn, true);
                    startColumn++;

                    cellNo = rowHeader.CreateCell(startColumn);
                    cellNo.SetCellValue(itemUa.Class);
                    cellNo.CellStyle = data.NormalStyleBody;
                    sheet.AutoSizeColumn(startColumn, true);
                    startColumn++;
                }

                if (i < listUnsubmited.Count())
                {
                    var itemUnsubmited = listUnsubmited[i];

                    startColumn = 8;

                    cellNo = rowHeader.CreateCell(startColumn);
                    cellNo.SetCellValue(i + 1);
                    cellNo.CellStyle = data.NormalStyleBody;
                    sheet.AutoSizeColumn(startColumn, true);
                    startColumn++;

                    cellNo = rowHeader.CreateCell(startColumn);
                    cellNo.SetCellValue(itemUnsubmited.TeacherId);
                    cellNo.CellStyle = data.NormalStyleBody;
                    sheet.AutoSizeColumn(startColumn, true);
                    startColumn++;

                    cellNo = rowHeader.CreateCell(startColumn);
                    cellNo.SetCellValue(itemUnsubmited.TeacherName);
                    cellNo.CellStyle = data.NormalStyleBody;
                    sheet.AutoSizeColumn(startColumn, true);
                    startColumn++;

                    cellNo = rowHeader.CreateCell(startColumn);
                    cellNo.SetCellValue(itemUnsubmited.Class);
                    cellNo.CellStyle = data.NormalStyleBody;
                    sheet.AutoSizeColumn(startColumn, true);
                    startColumn++;

                    cellNo = rowHeader.CreateCell(startColumn);
                    cellNo.SetCellValue(itemUnsubmited.Session);
                    cellNo.CellStyle = data.NormalStyleBody;
                    sheet.AutoSizeColumn(startColumn, true);
                    startColumn++;
                }

                if (i < data.ListTapping.Count())
                {
                    var itemTapping = data.ListTapping[i];

                    startColumn = 14;

                    cellNo = rowHeader.CreateCell(startColumn);
                    cellNo.SetCellValue(i + 1);
                    cellNo.CellStyle = data.NormalStyleBody;
                    sheet.AutoSizeColumn(startColumn, true);
                    startColumn++;

                    cellNo = rowHeader.CreateCell(startColumn);
                    cellNo.SetCellValue(itemTapping.IdStudent);
                    cellNo.CellStyle = data.NormalStyleBody;
                    sheet.AutoSizeColumn(startColumn, true);
                    startColumn++;

                    cellNo = rowHeader.CreateCell(startColumn);
                    cellNo.SetCellValue(NameUtil.GenerateFullName(itemTapping.FirstName, itemTapping.MiddleName, itemTapping.LastName));
                    cellNo.CellStyle = data.NormalStyleBody;
                    sheet.AutoSizeColumn(startColumn, true);
                    startColumn++;

                    cellNo = rowHeader.CreateCell(startColumn);
                    cellNo.SetCellValue(itemTapping.Class);
                    cellNo.CellStyle = data.NormalStyleBody;
                    sheet.AutoSizeColumn(startColumn, true);
                    startColumn++;
                }

                rowIndex++;
            }
            #endregion
            #endregion

            return default;
        }

        private async Task<string> GetSheet2(SheetRequest data)
        {
            var sheet = data.Workbook.CreateSheet("UA & Present Student");

            #region get Data
            var listPresentStudent = await GetPresentStudent(new PresentRequest
            {
                Body = data.Body,
                CancellationToken = data.CancellationToken,
                Date = data.Date,
                ListMappingAttendance = data.ListMappingAttendance,
                ListStudentStatus = data.ListStudentStatus,
                IdLessonUser = data.ListIdLessonUser,
                ListStudentEnrollmentUnion = data.ListStudentEnrollmentUnion,
                ListTapping = data.ListTapping,
            });

            var ListSession = listPresentStudent.SelectMany(e => e.ListSession.Select(f => f.Session)).Distinct().ToList();
            #endregion

            #region Add Logo
            if (data.Logo != null)
            {
                byte[] dataImg = data.Logo;
                int pictureIndex = data.Workbook.AddPicture(dataImg, PictureType.PNG);
                ICreationHelper helper = data.Workbook.GetCreationHelper();
                IDrawing drawing = sheet.CreateDrawingPatriarch();
                IClientAnchor anchor = helper.CreateClientAnchor();
                anchor.Col1 = 0;//0 index based column
                anchor.Row1 = 0;//0 index based row
                IPicture picture = drawing.CreatePicture(anchor, pictureIndex);
                picture.Resize(1.8, 2.5);
            }
            #endregion

            #region Row 1
            var rowIndex = 0;
            var rowHeader = sheet.CreateRow(rowIndex);

            int startColumn = 2;
            var cellNo = rowHeader.CreateCell(startColumn);
            var merge = new NPOI.SS.Util.CellRangeAddress(rowIndex, rowIndex, startColumn, startColumn + ListSession.Count() + 7);
            cellNo.SetCellValue("List of Students with UA & Present");
            cellNo.CellStyle = data.BoldStyleHeader;
            sheet.AddMergedRegion(merge);

            rowIndex++;
            #endregion

            #region Header Table
            rowIndex = 4;
            rowHeader = sheet.CreateRow(rowIndex);

            #region Hader Table Pertama
            List<string> header = new List<string>();
            header.Add("No");
            header.Add("Student ID");
            header.Add("Student Name");
            header.Add("Class");
            header.Add("Tapping Time");
            header.Add("Session");
            header.Add("Total Present");
            header.Add("Total Absent");
            header.Add("Total Late");

            startColumn = 2;
            foreach (var itemHeader in header)
            {
                if (itemHeader != "Session")
                {
                    merge = new NPOI.SS.Util.CellRangeAddress(rowIndex, rowIndex + 1, startColumn, startColumn);
                    cellNo = rowHeader.CreateCell(startColumn);
                    cellNo.SetCellValue(itemHeader);
                    cellNo.CellStyle = data.BoldStyle;
                    sheet.AutoSizeColumn(startColumn, true);
                    sheet.AddMergedRegion(merge);
                    startColumn++;
                }
                else
                {
                    merge = new NPOI.SS.Util.CellRangeAddress(rowIndex, rowIndex, startColumn, startColumn + ListSession.Count()-1);
                    cellNo = rowHeader.CreateCell(startColumn);
                    cellNo.SetCellValue(itemHeader);
                    cellNo.CellStyle = data.BoldStyle;
                    sheet.AutoSizeColumn(startColumn, true);
                    sheet.AddMergedRegion(merge);
                    startColumn++;

                    //for border
                    for (var i = 0; i <= ListSession.Count() -2; i++)
                    {
                        cellNo = rowHeader.GetCell(startColumn);
                        if(cellNo==null)
                            cellNo = rowHeader.CreateCell(startColumn);
                        cellNo.CellStyle = data.BoldStyle;
                        sheet.AutoSizeColumn(startColumn, true);
                        startColumn++;
                    }
                }
            }
            rowIndex++;
            #endregion

            #region Hader Table Ke-2
            rowHeader = sheet.CreateRow(rowIndex);

            startColumn = 2;
            for(var i =0;i<= ListSession.Count() + 8; i++)
            {
                if(i==5)
                {
                    foreach (var itemHeader in ListSession)
                    {
                        cellNo = rowHeader.CreateCell(startColumn);
                        cellNo.SetCellValue(itemHeader);
                        cellNo.CellStyle = data.BoldStyle;
                        sheet.AutoSizeColumn(startColumn, true);
                        startColumn++;
                        i++;
                    }
                }
                else
                {
                    cellNo = rowHeader.CreateCell(startColumn);
                    cellNo.CellStyle = data.BoldStyle;
                    sheet.AutoSizeColumn(startColumn, true);
                    startColumn++;
                }
                
            }
            rowIndex++;
            #endregion
            #endregion

            #region Body Table
            for (var i = 0; i < listPresentStudent.Count(); i++)
            {
                rowHeader = sheet.CreateRow(rowIndex);

                if (i < listPresentStudent.Count())
                {
                    var itemPresentStudent = listPresentStudent[i];
                    var listSessionStudent = itemPresentStudent.ListSession.OrderBy(e=>e.Session).ToList();
                    startColumn = 2;

                    cellNo = rowHeader.CreateCell(startColumn);
                    cellNo.SetCellValue(i + 1);
                    cellNo.CellStyle = data.NormalStyleBody;
                    sheet.AutoSizeColumn(startColumn, true);
                    startColumn++;

                    cellNo = rowHeader.CreateCell(startColumn);
                    cellNo.SetCellValue(itemPresentStudent.IdStudent);
                    cellNo.CellStyle = data.NormalStyleBody;
                    sheet.AutoSizeColumn(startColumn, true);
                    startColumn++;

                    cellNo = rowHeader.CreateCell(startColumn);
                    cellNo.SetCellValue(itemPresentStudent.StudentName);
                    cellNo.CellStyle = data.NormalStyleBody;
                    sheet.AutoSizeColumn(startColumn, true);
                    startColumn++;

                    cellNo = rowHeader.CreateCell(startColumn);
                    cellNo.SetCellValue(itemPresentStudent.Class);
                    cellNo.CellStyle = data.NormalStyleBody;
                    sheet.AutoSizeColumn(startColumn, true);
                    startColumn++;

                    cellNo = rowHeader.CreateCell(startColumn);
                    cellNo.SetCellValue(itemPresentStudent.Tapping.ToString());
                    cellNo.CellStyle = data.NormalStyleBody;
                    sheet.AutoSizeColumn(startColumn, true);
                    startColumn++;

                   foreach(var itemSession in listSessionStudent)
                   {
                        cellNo = rowHeader.CreateCell(startColumn);
                        cellNo.SetCellValue(itemSession.Status);
                        cellNo.CellStyle = data.NormalStyleBody;
                        sheet.AutoSizeColumn(startColumn, true);
                        startColumn++;
                   }

                    var countPreset = listSessionStudent.Where(e => e.Status == "V").Count();
                    cellNo = rowHeader.CreateCell(startColumn);
                    cellNo.SetCellValue(countPreset.ToString());
                    cellNo.CellStyle = data.NormalStyleBody;
                    sheet.AutoSizeColumn(startColumn, true);
                    startColumn++;

                    var countAbsent = listSessionStudent.Where(e => e.Status == "UA").Count();
                    cellNo = rowHeader.CreateCell(startColumn);
                    cellNo.SetCellValue(countAbsent.ToString());
                    cellNo.CellStyle = data.NormalStyleBody;
                    sheet.AutoSizeColumn(startColumn, true);
                    startColumn++;

                    var countLate = listSessionStudent.Where(e => e.Status == "LT").Count();
                    cellNo = rowHeader.CreateCell(startColumn);
                    cellNo.SetCellValue(countLate.ToString());
                    cellNo.CellStyle = data.NormalStyleBody;
                    sheet.AutoSizeColumn(startColumn, true);
                    startColumn++;
                }

                rowIndex++;
            }
            #endregion


            return default;
        }

        private async Task<string> GetSheet3(SheetRequest data)
        {
            var sheet = data.Workbook.CreateSheet("UA & Present - Details");

            #region Add Logo
            if (data.Logo != null)
            {
                byte[] dataImg = data.Logo;
                int pictureIndex = data.Workbook.AddPicture(dataImg, PictureType.PNG);
                ICreationHelper helper = data.Workbook.GetCreationHelper();
                IDrawing drawing = sheet.CreateDrawingPatriarch();
                IClientAnchor anchor = helper.CreateClientAnchor();
                anchor.Col1 = 0;//0 index based column
                anchor.Row1 = 0;//0 index based row
                IPicture picture = drawing.CreatePicture(anchor, pictureIndex);
                picture.Resize(1.8,3);
            }
            #endregion

            #region Row 1
            var rowIndex = 0;
            var rowHeader = sheet.CreateRow(rowIndex);
            int startColumn = 2;
            var cellNo = rowHeader.CreateCell(startColumn);
            var merge = new NPOI.SS.Util.CellRangeAddress(rowIndex, rowIndex, startColumn, startColumn + 7);
            cellNo.SetCellValue("List of Students with UA & Present - Details");
            cellNo.CellStyle = data.BoldStyleHeader;
            sheet.AddMergedRegion(merge);
            rowIndex++;
            #endregion

            #region Row 2
            rowHeader = sheet.CreateRow(rowIndex);
            startColumn = 2;
            cellNo = rowHeader.CreateCell(startColumn);
            merge = new NPOI.SS.Util.CellRangeAddress(rowIndex, rowIndex, startColumn, startColumn + 7);
            cellNo.SetCellValue($"as at {data.Date.ToString("dd MMMM yyyy HH:mm")}");
            sheet.AddMergedRegion(merge);
            cellNo.CellStyle = data.NormalStyleHeader;
            #endregion

            #region Header Table
            rowIndex = 4;
            rowHeader = sheet.CreateRow(rowIndex);

            List<string> header = new List<string>();
            header.Add("No");
            header.Add("Student ID");
            header.Add("Student Name");
            header.Add("Class");
            header.Add("Class ID");
            header.Add("Session");
            header.Add("Status");
            header.Add("Teacher");

            startColumn = 2;
            foreach (var itemHeader in header)
            {
                cellNo = rowHeader.CreateCell(startColumn);
                cellNo.SetCellValue(itemHeader);
                cellNo.CellStyle = data.BoldStyle;
                sheet.AutoSizeColumn(startColumn, true);
                startColumn++;
            }
            rowIndex++;
            #endregion

            #region Body Table
            #region List Of UA Student
            var listPresent = data.ListPresent.Where(e => e.Type == "UA").OrderBy(e => e.FirstName).ThenBy(e => e.MiddleName).ThenBy(e => e.LastName).ToList();

            rowIndex = 5;
            for (var i = 0; i < listPresent.Count(); i++)
            {
                rowHeader = sheet.CreateRow(rowIndex);

                if (i < listPresent.Count())
                {
                    var itemPresent = listPresent[i];

                    startColumn = 2;

                    cellNo = rowHeader.CreateCell(startColumn);
                    cellNo.SetCellValue(i + 1);
                    cellNo.CellStyle = data.NormalStyleBody;
                    sheet.AutoSizeColumn(startColumn, true);
                    startColumn++;

                    cellNo = rowHeader.CreateCell(startColumn);
                    cellNo.SetCellValue(itemPresent.IdStudent);
                    cellNo.CellStyle = data.NormalStyleBody;
                    sheet.AutoSizeColumn(startColumn, true);
                    startColumn++;

                    cellNo = rowHeader.CreateCell(startColumn);
                    cellNo.SetCellValue(NameUtil.GenerateFullName(itemPresent.FirstName, itemPresent.MiddleName, itemPresent.LastName));
                    cellNo.CellStyle = data.NormalStyleBody;
                    sheet.AutoSizeColumn(startColumn, true);
                    startColumn++;

                    cellNo = rowHeader.CreateCell(startColumn);
                    cellNo.SetCellValue(itemPresent.Class);
                    cellNo.CellStyle = data.NormalStyleBody;
                    sheet.AutoSizeColumn(startColumn, true);
                    startColumn++;

                    cellNo = rowHeader.CreateCell(startColumn);
                    cellNo.SetCellValue(itemPresent.ClassId);
                    cellNo.CellStyle = data.NormalStyleBody;
                    sheet.AutoSizeColumn(startColumn, true);
                    startColumn++;

                    cellNo = rowHeader.CreateCell(startColumn);
                    cellNo.SetCellValue(itemPresent.Session);
                    cellNo.CellStyle = data.NormalStyleBody;
                    sheet.AutoSizeColumn(startColumn, true);
                    startColumn++;

                    cellNo = rowHeader.CreateCell(startColumn);
                    cellNo.SetCellValue(itemPresent.Status);
                    cellNo.CellStyle = data.NormalStyleBody;
                    sheet.AutoSizeColumn(startColumn, true);
                    startColumn++;

                    cellNo = rowHeader.CreateCell(startColumn);
                    cellNo.SetCellValue(itemPresent.Teacher);
                    cellNo.CellStyle = data.NormalStyleBody;
                    sheet.AutoSizeColumn(startColumn, true);
                    startColumn++;
                }

                rowIndex++;
            }
            #endregion
            #endregion

            return default;
        }

        private async Task<string> GetSheet4(SheetRequest data)
        {
            var sheet = data.Workbook.CreateSheet("Summary");

            #region Add Logo
            if (data.Logo != null)
            {
                byte[] dataImg = data.Logo;
                int pictureIndex = data.Workbook.AddPicture(dataImg, PictureType.PNG);
                ICreationHelper helper = data.Workbook.GetCreationHelper();
                IDrawing drawing = sheet.CreateDrawingPatriarch();
                IClientAnchor anchor = helper.CreateClientAnchor();
                anchor.Col1 = 0;//0 index based column
                anchor.Row1 = 0;//0 index based row
                IPicture picture = drawing.CreatePicture(anchor, pictureIndex);
                picture.Resize(1.8, 3);
            }
            #endregion

            #region Row 1
            var rowIndex = 0;
            var rowHeader = sheet.CreateRow(rowIndex);
            int startColumn = 2;
            var cellNo = rowHeader.CreateCell(startColumn);
            var merge = new NPOI.SS.Util.CellRangeAddress(rowIndex, rowIndex, startColumn, startColumn + 8);
            cellNo.SetCellValue("Summary");
            cellNo.CellStyle = data.BoldStyleHeader;
            sheet.AddMergedRegion(merge);
            rowIndex++;
            #endregion

            #region Header Table
            rowIndex = 4;
            rowHeader = sheet.CreateRow(rowIndex);

            List<string> header = new List<string>();
            header.Add("Grade Level");
            header.Add("Class");
            header.Add("Student Name");
            header.Add("Class ID");
            header.Add("Session");

            startColumn = 2;
            foreach (var itemHeader in header)
            {
                cellNo = rowHeader.CreateCell(startColumn);
                cellNo.SetCellValue(itemHeader);
                cellNo.CellStyle = data.BoldStyle;
                sheet.AutoSizeColumn(startColumn, true);
                startColumn++;
            }
            rowIndex++;
            #endregion

            #region Body Table
            #region Summary
            var listPresent = data.ListPresent.Where(e => e.Type == "UA" && e.ClassId!=null).OrderBy(e => e.FirstName).ThenBy(e => e.MiddleName).ThenBy(e => e.LastName).ToList();

            rowIndex = 5;
            for (var i = 0; i < listPresent.Count(); i++)
            {
                rowHeader = sheet.CreateRow(rowIndex);

                if (i < listPresent.Count())
                {
                    var itemPresent = listPresent[i];

                    startColumn = 2;

                    cellNo = rowHeader.CreateCell(startColumn);
                    cellNo.SetCellValue(itemPresent.Grade);
                    cellNo.CellStyle = data.NormalStyleBody;
                    sheet.AutoSizeColumn(startColumn, true);
                    startColumn++;

                    cellNo = rowHeader.CreateCell(startColumn);
                    cellNo.SetCellValue(itemPresent.Class);
                    cellNo.CellStyle = data.NormalStyleBody;
                    sheet.AutoSizeColumn(startColumn, true);
                    startColumn++;

                    cellNo = rowHeader.CreateCell(startColumn);
                    cellNo.SetCellValue(NameUtil.GenerateFullName(itemPresent.FirstName, itemPresent.MiddleName, itemPresent.LastName));
                    cellNo.CellStyle = data.NormalStyleBody;
                    sheet.AutoSizeColumn(startColumn, true);
                    startColumn++;

                    cellNo = rowHeader.CreateCell(startColumn);
                    cellNo.SetCellValue(itemPresent.ClassId);
                    cellNo.CellStyle = data.NormalStyleBody;
                    sheet.AutoSizeColumn(startColumn, true);
                    startColumn++;

                    cellNo = rowHeader.CreateCell(startColumn);
                    cellNo.SetCellValue(itemPresent.Session);
                    cellNo.CellStyle = data.NormalStyleBody;
                    sheet.AutoSizeColumn(startColumn, true);
                    startColumn++;
                }

                rowIndex++;
            }
            #endregion

            #region Summary Data
            rowIndex = 4;
            startColumn = 8;

            rowHeader = sheet.GetRow(rowIndex);
            if(rowHeader==null)
                rowHeader = sheet.CreateRow(rowIndex);
            cellNo = rowHeader.CreateCell(startColumn);
            cellNo.SetCellValue("Unexcused Absences");
            cellNo.CellStyle = data.NormalStyleHeader;
            sheet.AutoSizeColumn(startColumn, true);
            rowIndex++;

            rowHeader = sheet.GetRow(rowIndex);
            if (rowHeader == null)
                rowHeader = sheet.CreateRow(rowIndex);
            cellNo = rowHeader.CreateCell(startColumn);
            cellNo.SetCellValue("Summary Data");
            cellNo.CellStyle = data.NormalStyleHeader;
            sheet.AutoSizeColumn(startColumn, true);
            rowIndex++;

            rowHeader = sheet.GetRow(rowIndex);
            if (rowHeader == null)
                rowHeader = sheet.CreateRow(rowIndex);
            cellNo = rowHeader.CreateCell(startColumn);
            cellNo.SetCellValue($"{data.Date.ToString("dddd, dd MMMM yyyy HH:mm")}");
            cellNo.CellStyle = data.BoldStyleBody;
            sheet.AutoSizeColumn(startColumn, true);
            rowIndex += 2;

            #region summary grade
            List<string> headerSummaryGrade = new List<string>();
            headerSummaryGrade.Add("Grade Level");
            headerSummaryGrade.Add("Total Session");

            rowHeader = sheet.GetRow(rowIndex);
            if (rowHeader == null)
                rowHeader = sheet.CreateRow(rowIndex);
            foreach (var itemHeader in headerSummaryGrade)
            {
                cellNo = rowHeader.GetCell(startColumn);
                if (cellNo == null)
                    cellNo = rowHeader.CreateCell(startColumn);
                cellNo.SetCellValue(itemHeader);
                cellNo.CellStyle = data.BoldStyle;
                sheet.AutoSizeColumn(startColumn, true);
                startColumn++;
            }
            rowIndex++;

            var summaryGrade = listPresent
                        .GroupBy(e => new
                        {
                            Grade = e.Grade
                        })
                        .ToList();

            var totalAll = listPresent.Count();

            foreach (var item in summaryGrade)
            {
                var grade = item.Key.Grade;
                var total = item.Count();
                startColumn = 8;

                rowHeader = sheet.GetRow(rowIndex);
                if (rowHeader == null)
                    rowHeader = sheet.CreateRow(rowIndex);

                cellNo = rowHeader.GetCell(startColumn);
                if(cellNo==null)
                    cellNo = rowHeader.CreateCell(startColumn);
                cellNo.SetCellValue($"Grade {grade}");
                cellNo.CellStyle = data.NormalStyleBody;
                sheet.AutoSizeColumn(startColumn, true);
                startColumn++;

                cellNo = rowHeader.GetCell(startColumn);
                if (cellNo == null)
                    cellNo = rowHeader.CreateCell(startColumn);
                cellNo.SetCellValue(total);
                cellNo.CellStyle = data.NormalStyleBody;
                sheet.AutoSizeColumn(startColumn, true);
                startColumn++;
                rowIndex++;
            }

            #region total summary grade
            startColumn = 8;

            rowHeader = sheet.GetRow(rowIndex);
            if (rowHeader == null)
                rowHeader = sheet.CreateRow(rowIndex);

            cellNo = rowHeader.GetCell(startColumn);
            if (cellNo == null)
                cellNo = rowHeader.CreateCell(startColumn);
            cellNo.SetCellValue("Total");
            cellNo.CellStyle = data.BoldStyle;
            sheet.AutoSizeColumn(startColumn, true);
            startColumn++;

            cellNo = rowHeader.GetCell(startColumn);
            if (cellNo == null)
                cellNo = rowHeader.CreateCell(startColumn);
            cellNo.SetCellValue(totalAll.ToString());
            cellNo.CellStyle = data.BoldStyle;
            sheet.AutoSizeColumn(startColumn, true);
            startColumn++;
            rowIndex++;
            #endregion
            #endregion

            #region summary session
            List<string> headerSummarySession = new List<string>();
            headerSummarySession.Add("Session");
            headerSummarySession.Add("Total Session");
            headerSummarySession.Add("Present");

            startColumn = 8;
            rowIndex+=3;
            rowHeader = sheet.GetRow(rowIndex);
            if (rowHeader == null)
                rowHeader = sheet.CreateRow(rowIndex);
            foreach (var itemHeader in headerSummarySession)
            {
                cellNo = rowHeader.GetCell(startColumn);
                if (cellNo == null)
                    cellNo = rowHeader.CreateCell(startColumn);
                cellNo.SetCellValue(itemHeader);
                cellNo.CellStyle = data.BoldStyle;
                sheet.AutoSizeColumn(startColumn, true);
                startColumn++;
            }
            rowIndex++;

            var summarySession = listPresent
                        .GroupBy(e => new
                        {
                            Session = e.Session
                        })
                        .OrderBy(e=>e.Key.Session)
                        .ToList();

            foreach (var item in summarySession)
            {
                var session = item.Key.Session;
                decimal total = item.Count();
                decimal avg = (decimal)Math.Round(total/totalAll, 2);

                startColumn = 8;
                rowHeader = sheet.GetRow(rowIndex);
                if (rowHeader == null)
                    rowHeader = sheet.CreateRow(rowIndex);

                cellNo = rowHeader.GetCell(startColumn);
                if (cellNo == null)
                    cellNo = rowHeader.CreateCell(startColumn);
                cellNo.SetCellValue($"Session {session}");
                cellNo.CellStyle = data.NormalStyleBody;
                sheet.AutoSizeColumn(startColumn, true);
                startColumn++;

                cellNo = rowHeader.GetCell(startColumn);
                if (cellNo == null)
                    cellNo = rowHeader.CreateCell(startColumn);
                cellNo.SetCellValue(total.ToString());
                cellNo.CellStyle = data.NormalStyleBody;
                sheet.AutoSizeColumn(startColumn, true);
                startColumn++;

                cellNo = rowHeader.GetCell(startColumn);
                if (cellNo == null)
                    cellNo = rowHeader.CreateCell(startColumn);
                cellNo.SetCellValue($"{avg.ToString()} %");
                cellNo.CellStyle = data.NormalStyleBody;
                sheet.AutoSizeColumn(startColumn, true);
                startColumn++;
                rowIndex++;
            }

            #region total summary grade
            startColumn = 8;

            rowHeader = sheet.GetRow(rowIndex);
            if (rowHeader == null)
                rowHeader = sheet.CreateRow(rowIndex);

            cellNo = rowHeader.GetCell(startColumn);
            if (cellNo == null)
                cellNo = rowHeader.CreateCell(startColumn);
            cellNo.SetCellValue("Total");
            cellNo.CellStyle = data.BoldStyle;
            sheet.AutoSizeColumn(startColumn, true);
            startColumn++;

            cellNo = rowHeader.GetCell(startColumn);
            if (cellNo == null)
                cellNo = rowHeader.CreateCell(startColumn);
            cellNo.SetCellValue(totalAll.ToString());
            cellNo.CellStyle = data.BoldStyle;
            sheet.AutoSizeColumn(startColumn, true);
            startColumn++;
            rowIndex++;
            #endregion
            #endregion
            #endregion
            #endregion

            return default;
        }

    }

}

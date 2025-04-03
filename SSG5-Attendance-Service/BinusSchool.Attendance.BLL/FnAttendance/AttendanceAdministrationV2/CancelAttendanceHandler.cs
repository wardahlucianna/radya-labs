using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Extensions;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using DocumentFormat.OpenXml.Spreadsheet;
using BinusSchool.Attendance.FnAttendance.AttendanceAdministration.validator;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceAdministration;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceAdministrationV2;
using BinusSchool.Attendance.FnAttendance.AttendanceAdministrationV2.Validator;
using BinusSchool.Persistence.AttendanceDb.Entities;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Common.Exceptions;
using System.Linq;
using BinusSchool.Data.Api.Attendance.FnAttendance;
using BinusSchool.Common.Model.Enums;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.Extensions.Azure;
using Microsoft.EntityFrameworkCore.Internal;
using BinusSchool.Auth.Authentications.Jwt;
using Microsoft.Azure.WebJobs;
using Newtonsoft.Json;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scoring.FnScoring.SendEmail.ApprovalByEmail;

namespace BinusSchool.Attendance.FnAttendance.AttendanceAdministrationV2
{
    public class CancelAttendanceHandler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;
        private readonly IAttendanceAdministrationV2 _serviceAttendanceAdmin;

        public CancelAttendanceHandler(IAttendanceDbContext dbContext, IAttendanceAdministrationV2 serviceAttendanceAdmin)
        {
            _dbContext = dbContext;
            _serviceAttendanceAdmin = serviceAttendanceAdmin;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<CancelAttendanceRequest, CancelAttendanceValidator>();

            var getAttendanceAdministration = await _dbContext.Entity<TrAttendanceAdministration>()
               .Include(e => e.StudentGrade).ThenInclude(e => e.Grade).ThenInclude(e => e.Level)
               .Include(e => e.AttdAdministrationCancel)
               .Where(e => e.Id == body.IdAttendanceAdministration)
               .Distinct()
               .FirstOrDefaultAsync(CancellationToken);

            if (getAttendanceAdministration == null)
                throw new BadRequestException($"attendance administration is not found");

            var idGrade = getAttendanceAdministration.StudentGrade.IdGrade;

            var homeroom = await _dbContext.Entity<MsHomeroomStudent>()
                    .Include(x => x.Homeroom)
                        .ThenInclude(x => x.GradePathwayClassroom)
                            .ThenInclude(x => x.Classroom)
                        .ThenInclude(x => x.GradePathwayClassrooms)
                            .ThenInclude(x => x.GradePathway)
                                .ThenInclude(x => x.Grade)
                    .Include(x => x.Homeroom)
                        .ThenInclude(x => x.AcademicYear)
                    .Where(x => x.IdStudent == body.IdStudent && x.Homeroom.Grade.Id == idGrade)
                    .Select(x => new
                    {
                        Id = x.Homeroom.Id,
                        AcademicYear = x.Homeroom.AcademicYear.Description,
                        Description = $"{x.Homeroom.GradePathwayClassroom.GradePathway.Grade.Code}{x.Homeroom.GradePathwayClassroom.Classroom.Code}",
                    })
                    .FirstOrDefaultAsync(CancellationToken);

            var homeroomTeacher = await _dbContext.Entity<MsHomeroomTeacher>()
                    .Include(x => x.Staff)
                    .Where(x => x.IdHomeroom == homeroom.Id)
                    .Select(x => new
                    {
                        IdUser = x.IdBinusian,
                        Name = NameUtil.GenerateFullName(x.Staff.FirstName, "", x.Staff.LastName)
                    })
                    .FirstOrDefaultAsync(CancellationToken);

            var listAttendanceAdminCancel = getAttendanceAdministration.AttdAdministrationCancel.Select(e=>e.IdScheduleLesson).ToList();

            var listIdScheduleLesson = new List<string>();
            #region Add Attendance Administration
            foreach (var IdScheduleLesson in body.IdScheduleLessons)
            {
                if (!listIdScheduleLesson.Contains(IdScheduleLesson))
                {
                    var newTrAttendanceAdminCancel = new TrAttdAdministrationCancel
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdAttendanceAdministration = body.IdAttendanceAdministration,
                        IdScheduleLesson = IdScheduleLesson
                    };
                    _dbContext.Entity<TrAttdAdministrationCancel>().Add(newTrAttendanceAdminCancel);
                    listIdScheduleLesson.Add(IdScheduleLesson);
                }
            }
            #endregion

            #region Add Attendance Administration cancel
            var getCancelAttendanceService = await _serviceAttendanceAdmin.GetCancelAttendance(new GetCancelAttendanceRequest
                        {
                            IdAttendanceAdministration = body.IdAttendanceAdministration,
                        });

            var GetCancelAttendance = getCancelAttendanceService.IsSuccess ? getCancelAttendanceService.Payload : null;
            var totalCancel = GetCancelAttendance.SelectMany(e=>e.ScheduleLessonCancels).Count();

            var getCancelAttendanceTotal = GetCancelAttendance
                                            .SelectMany(e => e.ScheduleLessonCancels)
                                            .Where(e => e.IsSessionDisabled)
                                            .Select(e=>e.IdScheduleLesson)
                                            .ToList();

            getCancelAttendanceTotal.AddRange(listIdScheduleLesson);
            getCancelAttendanceTotal = getCancelAttendanceTotal.Distinct().ToList();

            var SessionUsed = totalCancel - getCancelAttendanceTotal.Count();

            getAttendanceAdministration.SessionUsed = SessionUsed;
            _dbContext.Entity<TrAttendanceAdministration>().UpdateRange(getAttendanceAdministration);
            #endregion

            #region Update Attendance Entry
            var listAttendanceEntry = await _dbContext.Entity<TrAttendanceEntryV2>()
              .Where(e => body.IdScheduleLessons.Contains(e.IdScheduleLesson) && e.HomeroomStudent.IdStudent==body.IdStudent)
              .IgnoreQueryFilters()
              .ToListAsync(CancellationToken);

            var listAttendanceEntryGroup = listAttendanceEntry
                                          .GroupBy(e => new { e.IdScheduleLesson })
                                          .ToList();

            foreach (var item in listAttendanceEntryGroup)
            {
                var listAttendanceEntryByItem = item.OrderByDescending(e => e.DateIn).ToList();

                //non aktifkan
                var getAttendanceEntryNoActive = listAttendanceEntryByItem.FirstOrDefault();
                if (getAttendanceEntryNoActive != null)
                {
                    if (getAttendanceEntryNoActive.IsFromAttendanceAdministration)
                    {
                        getAttendanceEntryNoActive.IsActive = false;
                        _dbContext.Entity<TrAttendanceEntryV2>().Update(getAttendanceEntryNoActive);
                    }
                }

                //aktifkan data yang lama
                var getAttendanceEntryActive = listAttendanceEntryByItem.Where(e => e.PositionIn != "Admin").FirstOrDefault();
                if (getAttendanceEntryActive != null)
                {
                    getAttendanceEntryActive.IsActive = true;
                    _dbContext.Entity<TrAttendanceEntryV2>().Update(getAttendanceEntryActive);
                }
            }
            #endregion

            await _dbContext.SaveChangesAsync();

            #region send email
            var query = await _dbContext.Entity<TrAttendanceAdministration>()
                       .Include(x => x.Attendance)
                       .Include(x => x.StudentGrade)
                           .ThenInclude(x => x.Student)
                       .Include(x => x.StudentGrade)
                           .ThenInclude(x => x.Grade)
                               .ThenInclude(x => x.Level)
                       .Include(x => x.StudentGrade)
                           .ThenInclude(x => x.Grade)
                               .ThenInclude(x => x.Homerooms)
                                   .ThenInclude(x => x.GradePathwayClassroom)
                                       .ThenInclude(x => x.Classroom)
                       .Where(x => x.Id == body.IdAttendanceAdministration)
                       .Select(x => new
                       {
                           IdAttendanceAdministration = x.Id,
                           IdStudent = x.StudentGrade.Student.Id,
                           Level = x.StudentGrade.Grade.Level.Code,
                           IdLevel = x.StudentGrade.Grade.Level.Id,
                           IdGrade = x.StudentGrade.Grade.Id,
                           Grade = x.StudentGrade.Grade.Description,
                           Student = $"{NameUtil.GenerateFullNameWithId(x.StudentGrade.Student.Id, x.StudentGrade.Student.FirstName, x.StudentGrade.Student.MiddleName, x.StudentGrade.Student.LastName)}",
                           AttendanceCategory = x.Attendance.AbsenceCategory.Value.GetDescription(),
                           DetailStatus = x.Reason,
                           SubmittedDate = x.DateIn.Value.ToString("dd-MM-yyyy"),
                       })
                       .FirstOrDefaultAsync(CancellationToken);

            var listLessonBySchedule = await _dbContext.Entity<MsScheduleLesson>()
                                                .Where(e => body.IdScheduleLessons.Contains(e.Id))
                                                .SelectMany(e => e.Lesson.Schedules.Where(f => f.IdLesson == e.IdLesson && f.IdSession == e.IdSession && f.IdDay == e.IdDay && f.IdWeek == e.IdWeek).Select(f => new
                                                {
                                                    f.IdUser,
                                                    e.SessionID,
                                                    e.IdLesson,
                                                    e.ScheduleDate
                                                }))
                                                .OrderBy(e=>e.ScheduleDate).ThenBy(e=>e.SessionID)
                                                .ToListAsync(CancellationToken);

            List<ATD22NotificationModel> listEmailData = new List<ATD22NotificationModel> ();
            var htmlCancelAtd = string.Empty;
            if (homeroomTeacher != null)
            {
                List<NotifATD22> listNotifATD22 = new List<NotifATD22>();
                foreach (var item in listLessonBySchedule)
                {
                    var emailData = new NotifATD22
                    {
                        AcademicYear = homeroom.AcademicYear,
                        Level = query.Level,
                        Grade = query.Grade,
                        Homeroom = homeroom.Description,
                        StudentName = query.Student,
                        AttendanceCategory = query.AttendanceCategory,
                        DetailStatus = query.DetailStatus,
                        SubmittedDate = query.SubmittedDate,
                        CancelAttendance = item.ScheduleDate.ToString("dd MMM yyyy") + " - Session " + item.SessionID,
                    };

                    listNotifATD22.Add(emailData);
                }

                listEmailData.Add(new ATD22NotificationModel
                {
                    IdRecepient = homeroomTeacher.IdUser,
                    ListCancel = listNotifATD22,
                    IdLevel = query.IdLevel,
                });
            }

            var IdUserSubjectTeacher = listLessonBySchedule.Select(e=>e.IdUser).Distinct().ToList();

            if (IdUserSubjectTeacher.Any())
            {
                foreach (var idUser in IdUserSubjectTeacher)
                {
                    var listLessonBySubjectTeacher = listLessonBySchedule.Where(e => e.IdUser == idUser).ToList();

                    List<NotifATD22> listNotifATD22 = new List<NotifATD22>();
                    foreach (var item in listLessonBySubjectTeacher)
                    {
                        var emailData = new NotifATD22
                        {
                            AcademicYear = homeroom.AcademicYear,
                            Level = query.Level,
                            Grade = query.Grade,
                            Homeroom = homeroom.Description,
                            StudentName = query.Student,
                            AttendanceCategory = query.AttendanceCategory,
                            DetailStatus = query.DetailStatus,
                            SubmittedDate = query.SubmittedDate,
                            CancelAttendance = item.ScheduleDate.ToString("dd MMM yyyy") + " - Session " + item.SessionID,
                        };

                        listNotifATD22.Add(emailData);
                    }

                    listEmailData.Add(new ATD22NotificationModel
                    {
                        IdRecepient = idUser,
                        ListCancel = listNotifATD22,
                        IdLevel = query.IdLevel,
                    });
                }
            }

            if (KeyValues.ContainsKey("AttendanceAdministration"))
            {
                KeyValues.Remove("AttendanceAdministration");
            }
            KeyValues.Add("AttendanceAdministration", listEmailData);

            var listIdRecepient = listEmailData.Select(e => e.IdRecepient).ToList();
            var Notification = SendEmailNotifATD22(KeyValues, AuthInfo, listIdRecepient);
            #endregion

            return Request.CreateApiResult2();
        }

        public static string SendEmailNotifATD22(IDictionary<string, object> KeyValues, AuthenticationInfo AuthInfo, List<string> IdUserRecipient)
        {
            // send notification
            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "ATD22")
                {
                    IdRecipients = IdUserRecipient,
                    KeyValues = KeyValues
                });
                collector.Add(message);
            }
            return "";
        }
    }
}

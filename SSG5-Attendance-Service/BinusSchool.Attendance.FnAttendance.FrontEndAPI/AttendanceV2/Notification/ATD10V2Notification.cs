using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Attendance.FnAttendance.AttendanceEntry.Notification.ViewModels;
using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceV2;
using BinusSchool.Data.Model.Scheduling.FnSchedule.AppointmentBooking;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using BinusSchool.Persistence.AttendanceDb.Entities.User;
using FirebaseAdmin.Messaging;
using HandlebarsDotNet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SendGrid.Helpers.Mail;
using BinusSchool.Common.Abstractions;
namespace BinusSchool.Attendance.FnAttendance.AttendanceV2.Notification
{
    public class ATD10V2Notification : FunctionsNotificationHandler
    {
        private IDictionary<string, object> _notificationData;
        private readonly ILogger<ATD10V2Notification> _logger;
        private readonly IAttendanceDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly IMachineDateTime _datetime;
        private List<GetSubmmitedEmailResult> listAttendanceEmail;
        public ATD10V2Notification(INotificationManager notificationManager, IConfiguration configuration, ILogger<ATD10V2Notification> logger, IAttendanceDbContext dbContext, IMachineDateTime datetime, IDictionary<string, object> notificationData) :
           base("ATD10", notificationManager, configuration, logger)
        {
            _dbContext = dbContext;
            _configuration = configuration;
            _logger = logger;
            _datetime = datetime;

        }
        protected override Task FetchNotificationConfig()
        {
            // TODO: get config from actual source
            NotificationConfig = new NotificationConfig
            {
                EnEmail = true,
                EnPush = new EnablePushConfig
                {
                    Mobile = true,
                    Web = true
                }
            };

            return Task.CompletedTask;
        }

        protected override async Task Prepare()
        {
            try
            {
                var hostUrl = Configuration.GetSection("ClientApp:Web:Host").Get<string>();
                var attendanceUrlBase = $"{hostUrl}/attendance/detailsessionattendancev2";
                _notificationData = new Dictionary<string, object>();
                listAttendanceEmail = new List<GetSubmmitedEmailResult>();

                var Object = KeyValues.FirstOrDefault(e => e.Key == "listIdScheduleLessonEmail").Value;
                var listIdScheduleLessonEmail = JsonConvert.DeserializeObject<List<string>>(JsonConvert.SerializeObject(Object));

                var idAcademicYear = await _dbContext.Entity<MsScheduleLesson>()
                            .Where(e => listIdScheduleLessonEmail.Contains(e.Id))
                            .Select(e =>e.Grade.Level.IdAcademicYear)
                            .Distinct()
                            .FirstOrDefaultAsync(CancellationToken);

                var listSchedule = await _dbContext.Entity<MsSchedule>()
                            .Include(e => e.Lesson)
                            .Where(e => e.Lesson.Grade.Level.IdAcademicYear==idAcademicYear)
                            .Select(e => new
                            {
                                e.IdUser,
                                e.IdLesson,
                                e.IdSession,
                                e.IdWeek,
                                e.IdDay,
                            })
                            .Distinct()
                            .ToListAsync(CancellationToken);

                var listAttendanceEntryEmail = await _dbContext.Entity<TrAttendanceEntryV2>()
                                                .Include(e => e.AttendanceMappingAttendance).ThenInclude(e => e.Attendance)
                                                .Include(e => e.ScheduleLesson).ThenInclude(e => e.Subject)
                                                .Include(e => e.ScheduleLesson).ThenInclude(e => e.Lesson)
                                                .Include(e => e.ScheduleLesson).ThenInclude(e => e.Week)
                                                .Include(e => e.HomeroomStudent).ThenInclude(e => e.Student)
                                                .Where(e => listIdScheduleLessonEmail.Contains(e.IdScheduleLesson)
                                                )
                                                .Select(e => new
                                                {
                                                    e.IdScheduleLesson,
                                                    e.ScheduleLesson.IdAcademicYear,
                                                    e.HomeroomStudent.Homeroom.Semester,
                                                    e.ScheduleLesson.IdSubject,
                                                    e.ScheduleLesson.Subject.SubjectID,
                                                    e.ScheduleLesson.ScheduleDate,
                                                    e.ScheduleLesson.IdLesson,
                                                    e.ScheduleLesson.IdDay,
                                                    e.ScheduleLesson.IdWeek,
                                                    e.ScheduleLesson.IdSession,
                                                    week = e.ScheduleLesson.Week.Description,
                                                    createDate = e.DateUp == null ? e.DateIn : e.DateUp,
                                                    studentId = e.HomeroomStudent.IdStudent,
                                                    studentName = NameUtil.GenerateFullName(e.HomeroomStudent.Student.FirstName,
                                                                                         e.HomeroomStudent.Student.MiddleName,
                                                                                         e.HomeroomStudent.Student.LastName),
                                                    AttendanceName = e.AttendanceMappingAttendance.Attendance.Description,
                                                    ClassId = e.ScheduleLesson.Lesson.ClassIdGenerated
                                                })
                                                .ToListAsync(CancellationToken);

                var listScheduleLessonEmail = listAttendanceEntryEmail
                    .GroupBy(e => new
                    {
                        e.IdSubject,
                        e.IdLesson,
                        e.IdDay,
                        e.IdWeek,
                        e.week,
                        e.IdSession,
                        e.IdScheduleLesson,
                        e.IdAcademicYear,
                        e.Semester,
                        e.ClassId,
                        e.ScheduleDate,
                        e.SubjectID
                    })
                    .Select(e => e.Key).ToList();

                var listIdLessonEmail = listAttendanceEntryEmail.Select(e => e.IdLesson).ToList();
                var listIdSubjectEmail = listAttendanceEntryEmail.Select(e => e.IdSubject).ToList();
                var listIdDayEmail = listAttendanceEntryEmail.Select(e => e.IdDay).ToList();
                var listIdWeekEmail = listAttendanceEntryEmail.Select(e => e.IdWeek).ToList();

                foreach (var itemScheduleLesson in listScheduleLessonEmail)
                {
                    var listIdTeacher = listSchedule
                                    .Where(e => e.IdLesson == itemScheduleLesson.IdLesson
                                        && e.IdSession == itemScheduleLesson.IdSession
                                        && e.IdWeek == itemScheduleLesson.IdWeek
                                        && e.IdDay == itemScheduleLesson.IdDay
                                    )
                                    .Select(e => e.IdUser)
                                    .ToList();

                    var attendanceEmail = new GetSubmmitedEmailResult
                    {
                        idUserRecepient = listIdTeacher,
                        IdAcademicYear = itemScheduleLesson.IdAcademicYear,
                        Semester = itemScheduleLesson.Semester,
                        ClassId = itemScheduleLesson.ClassId,
                        Week = itemScheduleLesson.week,
                        ScheduleDate = itemScheduleLesson.ScheduleDate,
                        SubjectId = itemScheduleLesson.SubjectID,
                        AttendanceEntry = listAttendanceEntryEmail
                                            .Where(e => e.IdScheduleLesson == itemScheduleLesson.IdScheduleLesson)
                                            .GroupBy(e => new
                                            {
                                                e.studentName,
                                                e.studentId,
                                                e.createDate,
                                                e.AttendanceName
                                            })
                                            .Select(e => new AttendanceStudent
                                            {
                                                StudentName = e.Key.studentName,
                                                StudentId = e.Key.studentId,
                                                CreateDate = e.Key.createDate,
                                                AttendanceName = e.Key.AttendanceName,
                                            }).ToList(),
                    };

                    listAttendanceEmail.Add(attendanceEmail);
                }

                IdUserRecipients= listAttendanceEmail.SelectMany(e=>e.idUserRecepient).Distinct().ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }

        protected override async Task SendPushNotification()
        {
            var tokens = await _dbContext.Entity<MsUserPlatform>()
                .Where(x
                    => IdUserRecipients.Contains(x.IdUser) && x.FirebaseToken != null
                    && NotificationConfig.EnPush.AllowedPlatforms.Contains(x.AppPlatform))
                .Select(x => new { x.FirebaseToken, x.IdUser })
                .ToListAsync(CancellationToken);

            if (!EnsureAnyPushTokens(tokens.Select(e => e.FirebaseToken).ToList()))
                return;

            var SendPushNotificationTaks = new List<Task>();

            foreach (var idUser in IdUserRecipients)
            {
                var tokenByUser = tokens.Where(x => x.IdUser == idUser).Select(x => x.FirebaseToken).ToList();

                if (!EnsureAnyPushTokens(tokenByUser))
                    continue;

                var PushTemplate = Handlebars.Compile(NotificationTemplate.Push);
                var PushBody = PushTemplate(_notificationData);

                var TitleTemplate = Handlebars.Compile(NotificationTemplate.Title);
                var TitleBody = TitleTemplate(_notificationData);

                var message = new MulticastMessage
                {
                    Notification = new FirebaseAdmin.Messaging.Notification
                    {
                        Title = TitleBody,
                        Body = TitleBody
                    },
                    Tokens = tokenByUser,
                    Data = (IReadOnlyDictionary<string, string>)PushNotificationData
                };

                // send push notification
                SendPushNotificationTaks.Add(NotificationManager.SendPushNotification(message));
            }

            await Task.WhenAll(SendPushNotificationTaks);
        }

        protected override async Task SaveNotification(IEnumerable<string> idUserRecipients, bool isBlast)
        {
            try
            {
                if (idUserRecipients is null)
                {
                    _logger.LogInformation($"Skip sending notification. No Id User Recipients");
                    return;
                }

                var saveNotificationTasks = new List<Task>();

                foreach (var idUser in idUserRecipients)
                {
                    var PushTemplate = Handlebars.Compile(NotificationTemplate.Push);
                    var PushBody = PushTemplate(_notificationData);

                    var TitleTemplate = Handlebars.Compile(NotificationTemplate.Title);
                    var TitleBody = TitleTemplate(_notificationData);

                    saveNotificationTasks.Add(NotificationManager.SaveNotification(
                        CreateNotificationHistory(
                            new[] { idUser },
                            isBlast,
                            TitleBody,
                            PushBody)));
                }
                await Task.WhenAll(saveNotificationTasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }

        protected override async Task SendEmailNotification()
        {
            var User = await _dbContext.Entity<MsUser>()
                  .Where(x => IdUserRecipients.Contains(x.Id))
                  .Select(x => new
                  {
                      x.Id,
                      EmailAddress = new EmailAddress(x.Email, x.DisplayName)
                  })
                  .ToListAsync(CancellationToken);

            if (!User.Any())
                return;

            var sendEmailTasks = new List<Task>();
            foreach (var itemAttendance in listAttendanceEmail)
            {
                var listIdUser = itemAttendance.idUserRecepient.ToList();
                var GetUserById = User.Where(e => listIdUser.Contains(e.Id)).ToList();
                if (!GetUserById.Any())
                    continue;

                var dataTable = string.Empty;
                foreach (var itemAttendanceEntry in itemAttendance.AttendanceEntry)
                {
                    dataTable += "<tr>" +
                                    "<td>" + itemAttendanceEntry.StudentId + "</td>" +
                                    "<td>" + itemAttendanceEntry.StudentName + "</td>" +
                                    "<td>" + itemAttendanceEntry.AttendanceName + "</td>" +
                                    "<td>" + (itemAttendanceEntry.CreateDate.HasValue ? itemAttendanceEntry.CreateDate.Value.ToString("dddd, dd MMMM yyyy HH:mm") : "") + "</td>" +
                                "</tr>";
                }

                foreach (var user in GetUserById)
                {
                    _notificationData["receiverName"] = user.EmailAddress.Name;
                    _notificationData["academicYear"] = itemAttendance.IdAcademicYear;
                    _notificationData["semester"] = itemAttendance.Semester;
                    _notificationData["homeroom"] = itemAttendance.ClassId;
                    _notificationData["week"] = itemAttendance.Week;
                    _notificationData["classId"] = itemAttendance.SubjectId;
                    _notificationData["date"] = itemAttendance.ScheduleDate.ToString("dddd, dd MMMM yyyy");


                    var emailReplace = NotificationTemplate.Email.Replace("{{data}}", dataTable);
                    var EmailTemplate = Handlebars.Compile(emailReplace);
                    var EmailBody = EmailTemplate(_notificationData);

                    var TitleTemplate = Handlebars.Compile(NotificationTemplate.Title);
                    var TitleBody = TitleTemplate(_notificationData);


                    var message = new SendGridMessage
                    {
                        Subject = TitleBody,
                        Personalizations = new List<Personalization>
                        {
                            new Personalization { Tos = new List<EmailAddress> { user.EmailAddress } }
                        }
                    };

                    if (NotificationTemplate.EmailIsHtml)
                        message.HtmlContent = EmailBody;
                    else
                        message.PlainTextContent = EmailBody;

                    sendEmailTasks.Add(NotificationManager.SendEmail(message));
                }

            }
            // send batch email
            await Task.WhenAll(sendEmailTasks);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Api.Attendance.FnAttendance;
using BinusSchool.Data.Api.Scheduling.FnSchedule;
using BinusSchool.Data.Api.Student.FnStudent;
using BinusSchool.Data.Api.User.FnUser;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceAdministration;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceAdministrationV2;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummary;
using BinusSchool.Data.Model.Scheduling.FnSchedule.RolePosition;
using BinusSchool.Data.Model.User.FnUser.Register;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using BinusSchool.Persistence.AttendanceDb.Entities.User;
using DocumentFormat.OpenXml.Wordprocessing;
using FirebaseAdmin.Messaging;
using HandlebarsDotNet;
using Microsoft.Azure.Documents.SystemFunctions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NPOI.SS.Formula.Functions;
using SendGrid.Helpers.Mail;

namespace BinusSchool.Attendance.FnAttendance.AttendanceAdministrationV2.Notification
{
    public class ATD12V2Notification : FunctionsNotificationHandler
    {
        private IDictionary<string, object> _notificationData;
        private readonly ILogger<ATD12V2Notification> _logger;
        private readonly IAttendanceDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private List<ATD12V2NotificationModel> _dataEmail;
        private readonly IRegister _serviceRegister;
        private readonly IAttendanceAdministrationV2 _serviceAttendanceAdministration;

        public ATD12V2Notification(INotificationManager notificationManager, IConfiguration configuration, ILogger<ATD12V2Notification> logger, IAttendanceDbContext dbContext, IDictionary<string, object> notificationData, IRegister serviceRegister, IAttendanceAdministrationV2 serviceAttendanceAdministration) :
           base("ATD12", notificationManager, configuration, logger)
        {
            _dbContext = dbContext;
            _configuration = configuration;
            _logger = logger;
            _serviceRegister = serviceRegister;
            _serviceAttendanceAdministration = serviceAttendanceAdministration;
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
                _dataEmail = new List<ATD12V2NotificationModel>();
                _notificationData = new Dictionary<string, object>();
                List<string> listIdAttendanceAdmin = new List<string>();

                var Object = KeyValues.FirstOrDefault(e => e.Key == "listIdAttendanceAdmin").Value;
                listIdAttendanceAdmin = JsonConvert.DeserializeObject<List<string>>(JsonConvert.SerializeObject(Object));

                var school = await _dbContext.Entity<MsSchool>().Where(x => x.Id == IdSchool).FirstOrDefaultAsync(CancellationToken);
                if (school != null)
                    _notificationData["SchoolName"] = school.Name.ToUpper();

                var listAttendanceAdministration = await _dbContext.Entity<TrAttendanceAdministration>()
                                                    .Include(e => e.StudentGrade).ThenInclude(e => e.Grade).ThenInclude(e => e.Level)
                                                    .Include(e => e.StudentGrade).ThenInclude(e => e.Student)
                                                    .Include(e => e.Attendance)
                                                    .Where(e => listIdAttendanceAdmin.Contains(e.Id) && e.NeedValidation)
                                                    .Select(e => new
                                                    {
                                                        e.Id,
                                                        e.StudentGrade.Grade.Level.IdAcademicYear,
                                                        IdLevel = e.StudentGrade.Grade.IdLevel,
                                                        IdStudent = e.StudentGrade.Student.Id,
                                                        FirstName = e.StudentGrade.Student.FirstName,
                                                        MiddleName = e.StudentGrade.Student.MiddleName,
                                                        LastName = e.StudentGrade.Student.LastName,
                                                        e.StartDate,
                                                        e.EndDate,
                                                        Attendance = e.Attendance.Description,
                                                    })
                                                    .ToListAsync(CancellationToken);

                var listIdStudent = listAttendanceAdministration.Select(e => e.IdStudent).Distinct().ToList();
                var idAcademicYear = listAttendanceAdministration.Select(e => e.IdAcademicYear).FirstOrDefault();
                var StartDate = listAttendanceAdministration.Min(e => e.StartDate);

                var semester = await _dbContext.Entity<MsPeriod>()
                                         .Include(e => e.Grade).ThenInclude(e => e.Level)
                                         .Where(e => e.Grade.Level.AcademicYear.IdSchool == IdSchool
                                                    && (e.StartDate <= StartDate && e.EndDate >= StartDate))
                                         .Select(e => e.Semester)
                                         .FirstOrDefaultAsync(CancellationToken);

                var lisHomeroomStudent = await _dbContext.Entity<MsHomeroomStudent>()
                                .Include(e => e.Homeroom).ThenInclude(e => e.Grade)
                                .Include(e => e.Homeroom).ThenInclude(e => e.GradePathwayClassroom).ThenInclude(e => e.Classroom)
                                .Where(e => listIdStudent.Contains(e.IdStudent) && e.Homeroom.Semester == semester)
                                .Select(e => new
                                {
                                    IdStudent = e.IdStudent,
                                    Homeroom = e.Homeroom.Grade.Code + e.Homeroom.GradePathwayClassroom.Classroom.Code,
                                    Idhomeroom = e.IdHomeroom
                                })
                                .ToListAsync(CancellationToken);


                var lisMappingAttendance = await _dbContext.Entity<MsMappingAttendance>()
                                           .Where(e => e.Level.IdAcademicYear == idAcademicYear)
                                           .ToListAsync(CancellationToken);

                var idhomeroom = lisHomeroomStudent.Select(e => e.Idhomeroom).FirstOrDefault();

                var listAttendance = listAttendanceAdministration
                                 .Select(e => new ATD12V2NotificationAttendance
                                 {
                                     Id = e.Id,
                                     IdStudent = e.IdStudent,
                                     StudentName = NameUtil.GenerateFullName(e.FirstName, e.MiddleName, e.LastName),
                                     Homeroom = lisHomeroomStudent.Where(f => f.IdStudent == e.IdStudent).Select(e => e.Homeroom).FirstOrDefault(),
                                     AttendanceName = e.Attendance,
                                     DateRange = e.StartDate == e.EndDate ? $"{e.StartDate.ToString("dd MMM yyyy")} to {e.EndDate.ToString("dd MMM yyyy")}" : e.StartDate.ToString("dd MMM yyyy"),
                                 }).ToList();

                foreach (var item in listAttendance)
                {
                    GetCancelAttendanceRequest param = new GetCancelAttendanceRequest
                    {
                        IdAttendanceAdministration = item.Id
                    };

                    var getCancelAttendanceApi = await _serviceAttendanceAdministration.GetCancelAttendance(param);
                    var GetCancelAttendance = getCancelAttendanceApi.IsSuccess ? getCancelAttendanceApi.Payload : null;

                    if (GetCancelAttendance == null)
                        continue;

                    var absentTerms = lisMappingAttendance.Where(e => e.IdLevel == item.IdLevel).Select(e => e.AbsentTerms).FirstOrDefault();

                    var stringData = "<ul>";
                    foreach (var itemDay in GetCancelAttendance)
                    {
                        var day = itemDay.Date.ToString("dd MMM yyyy");

                        if (absentTerms == AbsentTerm.Session)
                            stringData += $"<li>{day}</li>";
                        else
                        {
                            foreach (var itemSession in itemDay.ScheduleLessonCancels)
                                stringData += $"<li>{day} (Session {itemSession.SessionId})</li>";
                        }


                    }
                    stringData += "</ul>";

                    item.Date = stringData;
                }

                var listHomeroomTeacher = await _dbContext.Entity<MsHomeroomTeacher>()
                                                .Where(e => e.IdHomeroom == idhomeroom)
                                                .Select(e => e.IdBinusian)
                                                .ToListAsync(CancellationToken);

                var listUser = await _dbContext.Entity<MsUser>()
                                    .Where(e => listHomeroomTeacher.Contains(e.Id))
                                    .ToListAsync(CancellationToken);

                _dataEmail = listHomeroomTeacher
                                .Select(e => new ATD12V2NotificationModel
                                {
                                    IdUserHomeroomTeacher = e,
                                    AttendanceAdmin = listAttendance,
                                    Email = listUser.Where(f => f.Id == e).Select(f => f.Email).FirstOrDefault()
                                })
                                .ToList();

                IdUserRecipients = _dataEmail.Select(x => x.IdUserHomeroomTeacher)
                                   .Distinct()
                                   .ToList();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }

        protected override async Task SendPushNotification()
        {
            if (IdUserRecipients is null)
                _logger.LogInformation($"Skip sending notification. No Id User Recipients");

            var SendPushNotificationTaks = new List<Task>();

            var paramToken = new GetFirebaseTokenRequest
            {
                IdUserRecipient = IdUserRecipients.ToList(),
            };

            var apiGetToken = await _serviceRegister.GetFirebaseToken(paramToken);
            var getFirebaseToken = apiGetToken.IsSuccess ? apiGetToken.Payload : null;
            if (getFirebaseToken == null)
                return;

            foreach (var idUser in IdUserRecipients)
            {
                var listAttendanceAdmin = _dataEmail.Where(e => e.IdUserHomeroomTeacher == idUser).FirstOrDefault();
                var listAttendance = listAttendanceAdmin.AttendanceAdmin;

                var tokenByUser = getFirebaseToken.Where(x => x.IdUser == idUser).ToList();
                var token = tokenByUser.Select(x => x.Token).ToList();

                if (!EnsureAnyPushTokens(token))
                    continue;

                foreach (var attendance in listAttendance)
                {
                    _notificationData["StudentName"] = attendance.StudentName;
                    _notificationData["ClassId"] = attendance.Homeroom;
                    _notificationData["BinussianID"] = attendance.IdStudent;
                    _notificationData["AttendanceStatus"] = attendance.AttendanceName;
                    _notificationData["DateRange"] = attendance.DateRange;
                    _notificationData["EmailReceiver"] = listAttendanceAdmin.Email;

                    var pushTitle = Handlebars.Compile(NotificationTemplate.Title);
                    var pushContent = Handlebars.Compile(NotificationTemplate.Push);

                    GeneratedTitle = pushTitle(_notificationData);
                    GeneratedContent = pushContent(_notificationData);

                    var message = new MulticastMessage
                    {
                        Notification = new FirebaseAdmin.Messaging.Notification
                        {
                            Title = GeneratedTitle,
                            Body = GeneratedContent
                        },
                        Tokens = token,
                        Data = (IReadOnlyDictionary<string, string>)PushNotificationData
                    };

                    // send push notification
                    SendPushNotificationTaks.Add(NotificationManager.SendPushNotification(message));
                }
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

                foreach (var idUser in IdUserRecipients)
                {
                    var listAttendanceAdmin = _dataEmail.Where(e => e.IdUserHomeroomTeacher == idUser).FirstOrDefault();
                    var listAttendance = listAttendanceAdmin.AttendanceAdmin;

                    foreach (var attendance in listAttendance)
                    {
                        _notificationData["StudentName"] = attendance.StudentName;
                        _notificationData["ClassId"] = attendance.Homeroom;
                        _notificationData["BinussianID"] = attendance.IdStudent;
                        _notificationData["AttendanceStatus"] = attendance.AttendanceName;
                        _notificationData["DateRange"] = attendance.DateRange;
                        _notificationData["EmailReceiver"] = listAttendanceAdmin.Email;

                        var pushTitle = Handlebars.Compile(NotificationTemplate.Title);
                        var pushContent = Handlebars.Compile(NotificationTemplate.Push);

                        GeneratedTitle = pushTitle(_notificationData);
                        GeneratedContent = pushContent(_notificationData);

                        saveNotificationTasks.Add(NotificationManager.SaveNotification(
                           CreateNotificationHistory(
                               new[] { idUser },
                               isBlast,
                               GeneratedTitle,
                               GeneratedContent)));
                    }
                }
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

            foreach (var item in User)
            {
                _notificationData["ReceiverName"] = item.EmailAddress.Name;

                var listAttendanceAdmin = _dataEmail.Where(e => e.IdUserHomeroomTeacher == item.Id).FirstOrDefault();
                var listAttendance = listAttendanceAdmin.AttendanceAdmin;

                foreach (var attendance in listAttendance)
                {
                    _notificationData["StudentName"] = attendance.StudentName;
                    _notificationData["ClassId"] = attendance.Homeroom;
                    _notificationData["BinussianID"] = attendance.IdStudent;
                    _notificationData["AttendanceStatus"] = attendance.AttendanceName;
                    _notificationData["DateRange"] = attendance.DateRange;
                    //_notificationData["Date"] = attendance.Date;
                    _notificationData["EmailReceiver"] = listAttendanceAdmin.Email;

                    var NotificationTemplateEmail = NotificationTemplate.Email.Replace("{{Date}}", attendance.Date);
                    var EmailTemplate = Handlebars.Compile(NotificationTemplateEmail);
                    var EmailBody = EmailTemplate(_notificationData);

                    var TitleTemplate = Handlebars.Compile(NotificationTemplate.Title);
                    var TitleBody = TitleTemplate(_notificationData);

                    var message = new SendGridMessage
                    {
                        Subject = TitleBody,
                        Personalizations = new List<Personalization>
                    {
                        new Personalization { Tos = User.Select(e=>e.EmailAddress).ToList() }
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

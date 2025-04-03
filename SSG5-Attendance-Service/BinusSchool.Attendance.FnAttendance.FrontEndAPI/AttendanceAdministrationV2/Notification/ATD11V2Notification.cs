using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Api.Scheduling.FnSchedule;
using BinusSchool.Data.Api.Student.FnStudent;
using BinusSchool.Data.Api.User.FnUser;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceAdministration;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceAdministrationV2;
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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SendGrid.Helpers.Mail;

namespace BinusSchool.Attendance.FnAttendance.AttendanceAdministrationV2.Notification
{
    public class ATD11V2Notification : FunctionsNotificationHandler
    {
        private IDictionary<string, object> _notificationData;
        private readonly ILogger<ATD11V2Notification> _logger;
        private readonly IAttendanceDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private List<ATD11V2NotificationModel> _dataEmail;
        private readonly IRolePosition _serviceRolePosition;
        private readonly IRegister _serviceRegister;

        public ATD11V2Notification(INotificationManager notificationManager, IConfiguration configuration, ILogger<ATD11V2Notification> logger, IAttendanceDbContext dbContext, IDictionary<string, object> notificationData, IRolePosition serviceRolePosition, IRegister serviceRegister) :
           base("ATD11", notificationManager, configuration, logger)
        {
            _dbContext = dbContext;
            _configuration = configuration;
            _logger = logger;
            _serviceRolePosition = serviceRolePosition;
            _serviceRegister = serviceRegister;
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
                _dataEmail = new List<ATD11V2NotificationModel>();
                var hostUrl = Configuration.GetSection("ClientApp:Web:Host").Get<string>();
                var attendanceUrlBase = $"{hostUrl}/attendance/detailattendanceapprovalv2";
                _notificationData = new Dictionary<string, object>();
                List<string> listIdAttendanceAdmin = new List<string>();

                var school = await _dbContext.Entity<MsSchool>().Where(x => x.Id == IdSchool).FirstOrDefaultAsync(CancellationToken);
                if (school != null)
                    _notificationData["schoolName"] = school.Name.ToUpper();

                var Object = KeyValues.FirstOrDefault(e => e.Key == "listIdAttendanceAdmin").Value;
                listIdAttendanceAdmin = JsonConvert.DeserializeObject<List<string>>(JsonConvert.SerializeObject(Object));

                var listAttendanceAdministration = await _dbContext.Entity<TrAttendanceAdministration>()
                                                    .Include(e => e.StudentGrade).ThenInclude(e => e.Grade).ThenInclude(e => e.Level)
                                                    .Include(e => e.StudentGrade).ThenInclude(e => e.Student)
                                                    .Include(e => e.Attendance)
                                                    .Where(e => listIdAttendanceAdmin.Contains(e.Id) && e.NeedValidation)
                                                    .Select(e => new
                                                    {
                                                        e.Id,
                                                        e.IdStudentGrade,
                                                        e.StudentGrade.IdGrade,
                                                        e.StudentGrade.Grade.IdLevel,
                                                        e.StudentGrade.Grade.Level.IdAcademicYear,
                                                        Grade = e.StudentGrade.Grade.Code,
                                                        IdStudent = e.StudentGrade.Student.Id,
                                                        FirstName = e.StudentGrade.Student.FirstName,
                                                        MiddleName = e.StudentGrade.Student.MiddleName,
                                                        LastName = e.StudentGrade.Student.LastName,
                                                        e.StartDate,
                                                        e.EndDate,
                                                        e.StartTime,
                                                        e.EndTime,
                                                        e.Attendance.ExcusedAbsenceCategory,
                                                        Attendance = e.Attendance.Description,
                                                        e.AbsencesFile,
                                                        e.Reason,
                                                        e.UserIn
                                                    })
                                                    .ToListAsync(CancellationToken);

                var listIdStudent = listAttendanceAdministration.Select(e => e.IdStudent).Distinct().ToList();
                var listIdUserCreate = listAttendanceAdministration.Select(e => e.UserIn).Distinct().ToList();
                var idAcademicYear = listAttendanceAdministration.Select(e => e.IdAcademicYear).FirstOrDefault();
                var idGrade = listAttendanceAdministration.Select(e => e.IdGrade).FirstOrDefault();
                var idLevel = listAttendanceAdministration.Select(e => e.IdLevel).FirstOrDefault();
                var StartDate = listAttendanceAdministration.Min(e => e.StartDate);

                var semester = await _dbContext.Entity<MsPeriod>()
                                         .Include(e => e.Grade).ThenInclude(e => e.Level)
                                         .Where(e => e.Grade.Level.AcademicYear.IdSchool == IdSchool
                                                    && (e.StartDate <= StartDate && e.EndDate >= StartDate))
                                         .Select(e => e.Semester)
                                         .FirstOrDefaultAsync(CancellationToken);

                var listUser = await _dbContext.Entity<MsUser>()
                                .Where(e => listIdUserCreate.Contains(e.Id))
                                .ToListAsync(CancellationToken);

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

                var idhomeroom = lisHomeroomStudent.Select(e => e.Idhomeroom).FirstOrDefault();

                var listAttendance = listAttendanceAdministration
                                 .Select(e => new ATD11V2NotificationAttendance
                                 {
                                     IdStudent = e.IdStudent,
                                     StudentName = NameUtil.GenerateFullName(e.FirstName, e.MiddleName, e.LastName),
                                     Grade = e.Grade,
                                     Homeroom = lisHomeroomStudent.Where(f => f.IdStudent == e.IdStudent).Select(e => e.Homeroom).FirstOrDefault(),
                                     Date = $"{e.StartDate.ToString("dd MMM yyyy")} - {e.EndDate.ToString("dd MMM yyyy")}",
                                     Time = $"{e.StartTime.ToString(@"hh\:mm")} - {e.EndDate.ToString(@"hh\:mm")}",
                                     Category = e.ExcusedAbsenceCategory.ToString(),
                                     AttendanceStatus = e.Attendance,
                                     File = e.AbsencesFile,
                                     Note = e.Reason,
                                     AbsentBy = listUser.Where(f => f.Id == e.UserIn).Select(e => e.DisplayName).FirstOrDefault(),
                                     Url = $"{attendanceUrlBase}?id={e.Id}",
                                     Id = e.Id,
                                 }).ToList();

                var idRoleApproval = await _dbContext.Entity<MsApprovalAttendanceAdministration>()
                                        .Where(e => e.IdSchool == IdSchool)
                                        .FirstOrDefaultAsync(CancellationToken);

                var param = new GetUserSubjectByEmailRecepientRequest
                {
                    IdAcademicYear = idAcademicYear,
                    IdSchool = IdSchool,
                    IdLevel = idLevel,
                    IdGrade = idGrade,
                    IdHomeroom = idhomeroom,
                    IsShowIdUser = true,
                    EmailRecepients = new List<GetUserEmailRecepient>() { new GetUserEmailRecepient
                    {
                        IdRole = idRoleApproval.IdRole,
                    } }
                };

                var apiGetUserSubjectByEmailRecepient = await _serviceRolePosition.GetUserSubjectByEmailRecepient(param);
                var getUserSubjectByEmailRecepient = apiGetUserSubjectByEmailRecepient.IsSuccess ? apiGetUserSubjectByEmailRecepient.Payload : null;

                if (getUserSubjectByEmailRecepient == null)
                    return;

                _dataEmail = getUserSubjectByEmailRecepient
                                .Select(e => new ATD11V2NotificationModel
                                {
                                    IdUserApproval = e.IdUser,
                                    AttendanceAdmin = listAttendance
                                })
                                .ToList();

                IdUserRecipients = _dataEmail.Select(x => x.IdUserApproval)
                                   .Distinct()
                                   .ToList();

                GeneratedContent = NotificationTemplate.Push;
                PushNotificationData["action"] = "EA_APPROVAL";
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
                var listAttendance = _dataEmail.Where(e => e.IdUserApproval == idUser).SelectMany(e => e.AttendanceAdmin).ToList();

                var tokenByUser = getFirebaseToken.Where(x => x.IdUser == idUser).ToList();
                var token = tokenByUser.Select(x => x.Token).ToList();

                if (!EnsureAnyPushTokens(token))
                    continue;

                foreach (var attendance in listAttendance)
                {
                    _notificationData["studentName"] = attendance.StudentName;

                    var pushTitle = Handlebars.Compile(NotificationTemplate.Title);
                    GeneratedTitle = pushTitle(_notificationData);

                    PushNotificationData["Id"] = attendance.Id;
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

                foreach (var idUser in idUserRecipients)
                {
                    var listAttendance = _dataEmail.Where(e => e.IdUserApproval == idUser).SelectMany(e => e.AttendanceAdmin).ToList();

                    foreach (var attendance in listAttendance)
                    {
                        _notificationData["studentName"] = attendance.StudentName;

                        var pushTitle = Handlebars.Compile(NotificationTemplate.Title);
                        GeneratedTitle = pushTitle(_notificationData);

                        saveNotificationTasks.Add(NotificationManager.SaveNotification(
                            CreateNotificationHistory(
                                new[] { idUser },
                                isBlast,
                                GeneratedTitle,
                                GeneratedContent, attendance.Id)));
                    }
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

            foreach (var item in User)
            {
                _notificationData["receiverName"] = item.EmailAddress.Name;
                var listAttendance = _dataEmail.Where(e => e.IdUserApproval == item.Id).SelectMany(e => e.AttendanceAdmin).ToList();

                foreach (var Attendance in listAttendance)
                {
                    _notificationData["studentName"] = Attendance.StudentName;
                    _notificationData["binusianId"] = Attendance.IdStudent;
                    _notificationData["grade"] = Attendance.Grade;
                    _notificationData["class"] = Attendance.Homeroom;
                    _notificationData["date"] = Attendance.Date;
                    _notificationData["time"] = Attendance.Time;
                    _notificationData["category"] = Attendance.Category;
                    _notificationData["file"] = Attendance.File;
                    _notificationData["note"] = Attendance.Note;
                    _notificationData["Url"] = Attendance.Url;
                    _notificationData["attendanceStatus"] = Attendance.AttendanceStatus;

                    var EmailTemplate = Handlebars.Compile(NotificationTemplate.Email);
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

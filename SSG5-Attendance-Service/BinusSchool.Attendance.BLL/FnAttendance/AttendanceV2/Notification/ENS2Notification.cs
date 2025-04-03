using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Api.Attendance.FnAttendance;
using BinusSchool.Data.Api.Scheduling.FnSchedule;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceV2;
using BinusSchool.Data.Model.Scheduling.FnSchedule.RolePosition;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using BinusSchool.Persistence.AttendanceDb.Entities.Teaching;
using BinusSchool.Persistence.AttendanceDb.Entities.User;
using FirebaseAdmin.Messaging;
using HandlebarsDotNet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SendGrid.Helpers.Mail;

namespace BinusSchool.Attendance.FnAttendance.AttendanceV2.Notification
{
    public class ENS2Notification : FunctionsNotificationHandler
    {
        private IDictionary<string, object> _notificationData;
        private readonly ILogger<ENS2Notification> _logger;
        private readonly IConfiguration _configuration;
        private readonly IMachineDateTime _datetime;
        private readonly IAttendanceDbContext _dbContext;
        private readonly IAttendanceV2 _attendanceService;
        private readonly IRolePosition _rolePositionService;
        private List<ENS2NotificationRequest> listAttendanceEmail = new List<ENS2NotificationRequest>();
        public ENS2Notification(INotificationManager notificationManager, IConfiguration configuration, ILogger<ENS2Notification> logger, IAttendanceDbContext dbContext, IMachineDateTime datetime, IDictionary<string, object> notificationData, IAttendanceV2 attendanceService, IRolePosition rolePositionService) :
           base("ENS2", notificationManager, configuration, logger)
        {
            _dbContext = dbContext;
            _configuration = configuration;
            _logger = logger;
            _datetime = datetime;
            _attendanceService = attendanceService;
            _rolePositionService = rolePositionService;
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
                _notificationData = new Dictionary<string, object>();

                var startdate = _datetime.ServerTime.ToString();
                _logger.LogWarning("[Timer] {0} for school {1} is running", _datetime.ServerTime.ToString(), IdSchool);

                var schoolName = await _dbContext.Entity<MsSchool>()
                                            .Where(e => e.Id == IdSchool
                                                    )
                                            .Select(e => e.Name)
                                            .FirstOrDefaultAsync(CancellationToken);

                _notificationData["SchoolName"] = schoolName;

                var idAcademicYear = await _dbContext.Entity<MsPeriod>()
                                            .Include(e => e.Grade).ThenInclude(e => e.Level)
                                            .Where(e =>
                                                        e.Grade.Level.AcademicYear.IdSchool == IdSchool
                                                        && (e.StartDate <= _datetime.ServerTime.Date && e.EndDate >= _datetime.ServerTime.Date)
                                                    )
                                            .Select(e => e.Grade.Level.IdAcademicYear)
                                            .FirstOrDefaultAsync(CancellationToken);


                var listIdUser = await _dbContext.Entity<MsHomeroomTeacher>()
                                            .Include(e => e.Staff)
                                            .Where(e => e.Homeroom.IdAcademicYear == idAcademicYear
                                                        && e.IsAttendance
                                                        && e.Homeroom.Grade.Level.MappingAttendances.Any(f => f.AbsentTerms == AbsentTerm.Session))
                                            .Select(e => e.Staff)
                                            .Distinct()
                                            .ToListAsync(CancellationToken);

                List<Ens4NotificationUnsubmited> listPending = new List<Ens4NotificationUnsubmited>();
                var endDate = startdate;
                foreach (var itemStaff in listIdUser)
                {
                    var index = listIdUser.IndexOf(itemStaff) + 1;
                    _logger.LogWarning($"Loading...{index}/{listIdUser.Count()} >> {endDate} | {_datetime.ServerTime.ToString()}");
                    endDate = _datetime.ServerTime.ToString();
                    var GetUnsubmittedAttendanceV2Service = await _attendanceService.GetPendingAttendanceV2(new GetUnresolvedAttendanceV2Request
                    {
                        IdAcademicYear = idAcademicYear,
                        CurrentPosition = "CA",
                        IdUser = itemStaff.IdBinusian
                    });

                    var GetUnsubmittedAttendanceV2 = GetUnsubmittedAttendanceV2Service.IsSuccess ? GetUnsubmittedAttendanceV2Service.Payload : null;

                    if (GetUnsubmittedAttendanceV2.Attendances != null)
                    {
                        var newUnsubmittedAttendance = GetUnsubmittedAttendanceV2.Attendances
                                                        .Select(e => new Ens4NotificationUnsubmited
                                                        {
                                                            ClassId = e.ClassID,
                                                            Datetime = e.Date,
                                                            Homeroom = e.Homeroom.Description,
                                                            Session = e.Session.Description,
                                                            IdLesson = e.IdLesson,
                                                            IdTeacher = itemStaff.IdBinusian,
                                                            Teacher = NameUtil.GenerateFullName(itemStaff.FirstName, itemStaff.LastName),
                                                            TotalStudent = e.TotalStudent
                                                        }).ToList();

                        listPending.AddRange(newUnsubmittedAttendance);
                    }
                }

                #region get data teacher assignment
                var listTeacherPosition = await _dbContext.Entity<MsTeacherPosition>()
                                            .Where(e => e.IdSchool == IdSchool
                                                        && (e.LtPosition.Code == PositionConstant.LevelHead
                                                            || e.LtPosition.Code == PositionConstant.AffectiveCoordinator))
                                            .Select(e => e.Id)
                                            .Distinct()
                                            .ToListAsync(CancellationToken);

                var listIdUserByPosition = await _dbContext.Entity<TrNonTeachingLoad>()
                                            .Where(e => e.NonTeachingLoad.IdAcademicYear == idAcademicYear
                                                        && (e.NonTeachingLoad.TeacherPosition.LtPosition.Code == PositionConstant.LevelHead
                                                            || e.NonTeachingLoad.TeacherPosition.LtPosition.Code == PositionConstant.AffectiveCoordinator))
                                            .Select(e => e.IdUser)
                                            .Distinct()
                                            .ToListAsync(CancellationToken);

                foreach (var idUser in listIdUserByPosition)
                {
                    var GetSubjectByUserService = await _rolePositionService.GetSubjectByUser(new GetSubjectByUserRequest
                    {
                        IdAcademicYear = idAcademicYear,
                        IdUser = idUser,
                        ListIdTeacherPositions = listTeacherPosition,
                    });

                    var GetSubjectByUser = GetSubjectByUserService.IsSuccess ? GetSubjectByUserService.Payload : null;

                    if (GetSubjectByUser != null)
                    {
                        var listIdLesson = GetSubjectByUser
                                            .Select(e => e.Lesson.Id).Distinct()
                                            .ToList();

                        var listUnsubmitedByUserPosition = listPending
                                                            .GroupBy(e => new
                                                            {
                                                                e.ClassId,
                                                                e.Datetime,
                                                                e.Homeroom,
                                                                e.Session,
                                                                IdTeacher = e.IdTeacher,
                                                                Teacher = e.Teacher,
                                                                e.IdLesson,
                                                                e.TotalStudent
                                                            })
                                                            .Select(e => new Ens4NotificationUnsubmited
                                                            {
                                                                ClassId = e.Key.ClassId,
                                                                Date = e.Key.Datetime.ToString("dd/MM/yy"),
                                                                Homeroom = e.Key.Homeroom,
                                                                Session = e.Key.Session,
                                                                IdTeacher = e.Key.IdTeacher,
                                                                Teacher = e.Key.Teacher,
                                                                TotalStudent = e.Key.TotalStudent,
                                                                IdLesson = e.Key.IdLesson
                                                            })
                                                            .Where(e => listIdLesson.Contains(e.IdLesson))
                                                            .OrderBy(e => e.Date).ThenBy(e => e.Homeroom).ThenBy(e => e.Session)
                                                            .ToList();

                        if (listUnsubmitedByUserPosition.Any())
                            listAttendanceEmail.Add(new ENS2NotificationRequest
                            {
                                IdUser = idUser,
                                Pending = listUnsubmitedByUserPosition
                            });
                    }
                }
                #endregion

                IdUserRecipients = listAttendanceEmail.Select(e => e.IdUser).ToList();
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
                .Select(x => x.FirebaseToken)
                .ToListAsync(CancellationToken);

            if (!EnsureAnyPushTokens(tokens))
                return;

            var SendPushNotificationTaks = new List<Task>();

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
                Tokens = tokens,
                Data = (IReadOnlyDictionary<string, string>)PushNotificationData
            };

            // send push notification
            SendPushNotificationTaks.Add(NotificationManager.SendPushNotification(message));

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

                var PushTemplate = Handlebars.Compile(NotificationTemplate.Push);
                var PushBody = PushTemplate(_notificationData);

                var TitleTemplate = Handlebars.Compile(NotificationTemplate.Title);
                var TitleBody = TitleTemplate(_notificationData);

                saveNotificationTasks.Add(NotificationManager.SaveNotification(
                    CreateNotificationHistory(
                        idUserRecipients,
                        isBlast,
                        TitleBody,
                        PushBody)));
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
                var listIdUser = itemAttendance.IdUser;
                var GetUserById = User.Where(e => e.Id == listIdUser).FirstOrDefault();
                if (GetUserById == null)
                    continue;

                _notificationData["RecipientName"] = GetUserById.EmailAddress.Name;
                _notificationData["Data"] = itemAttendance.Pending;

                var EmailTemplate = Handlebars.Compile(NotificationTemplate.Email);
                var EmailBody = EmailTemplate(_notificationData);

                var TitleTemplate = Handlebars.Compile(NotificationTemplate.Title);
                var TitleBody = TitleTemplate(_notificationData);


                var message = new SendGridMessage
                {
                    Subject = TitleBody,
                    Personalizations = new List<Personalization>
                    {
                        new Personalization { Tos = new List<EmailAddress> { GetUserById.EmailAddress } }
                    }
                };

                if (NotificationTemplate.EmailIsHtml)
                    message.HtmlContent = EmailBody;
                else
                    message.PlainTextContent = EmailBody;

                sendEmailTasks.Add(NotificationManager.SendEmail(message));

            }
            // send batch email
            await Task.WhenAll(sendEmailTasks);
        }
    }
}

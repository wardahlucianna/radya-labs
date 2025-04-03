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
using BinusSchool.Persistence.AttendanceDb.Entities.Student;
using BinusSchool.Persistence.AttendanceDb.Entities.Teaching;
using BinusSchool.Persistence.AttendanceDb.Entities.User;
using FirebaseAdmin.Messaging;
using FluentEmail.Core;
using HandlebarsDotNet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SendGrid.Helpers.Mail;

namespace BinusSchool.Attendance.FnAttendance.AttendanceV2.Notification
{
    public class ENS3Notification : FunctionsNotificationHandler
    {
        private IDictionary<string, object> _notificationData;
        private readonly ILogger<ENS3Notification> _logger;
        private readonly IConfiguration _configuration;
        private readonly IMachineDateTime _datetime;
        private readonly IAttendanceDbContext _dbContext;
        private readonly IAttendanceV2 _attendanceService;
        private readonly IRolePosition _rolePositionService;
        private List<ENS3NotificationRequest> listAttendanceEmail = new List<ENS3NotificationRequest>();
        public ENS3Notification(INotificationManager notificationManager, IConfiguration configuration, ILogger<ENS3Notification> logger, IAttendanceDbContext dbContext, IMachineDateTime datetime, IDictionary<string, object> notificationData, IAttendanceV2 attendanceService, IRolePosition rolePositionService) :
           base("ENS3", notificationManager, configuration, logger)
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

                var listEvent = await _dbContext.Entity<TrEventIntendedForAtdPICStudent>()
                                            .Include(e => e.EventIntendedForAttendanceStudent).ThenInclude(e=>e.EventIntendedFor)
                                            .Include(e => e.User)
                                            .Where(e => e.EventIntendedForAttendanceStudent.EventIntendedFor.Event.IdAcademicYear == idAcademicYear
                                                        && e.EventIntendedForAttendanceStudent.EventIntendedFor.Event.StatusEvent == "Approved"
                                                        && e.EventIntendedForAttendanceStudent.EventIntendedFor.IntendedFor == "STUDENT"
                                                        && e.EventIntendedForAttendanceStudent.Type == EventIntendedForAttendanceStudent.Mandatory
                                                        && e.EventIntendedForAttendanceStudent.EventIntendedFor.Event.EventDetails.Any(e => e.StartDate.Date <= _datetime.ServerTime.Date)
                                                        )
                                            .Distinct()
                                            .ToListAsync(CancellationToken);

                var listIdUser = listEvent.Select(e => e.User).Distinct().ToList();
                var listIdEvent = listEvent.Select(e => e.EventIntendedForAttendanceStudent.EventIntendedFor.IdEvent).Distinct().ToList();

                var listUserEvent = await _dbContext.Entity<TrUserEvent>()
                                            .Include(e=>e.EventDetail)
                                            .Where(e => listIdEvent.Contains(e.EventDetail.IdEvent))
                                            .ToListAsync(CancellationToken);

                var listStudentGrade = await _dbContext.Entity<MsStudentGrade>()
                                            .Where(e => e.Grade.Level.IdAcademicYear==idAcademicYear)
                                            .ToListAsync(CancellationToken);

                var listStudentGradeEvent = listUserEvent
                                            .Select(e => new
                                            {
                                                idEvent = e.EventDetail.IdEvent,
                                                IdUser = e.IdUser,
                                                IdGrade = listStudentGrade.Where(f => f.IdStudent == e.IdUser).Select(e => e.IdGrade).FirstOrDefault()
                                            })
                                            .Where(e=>e.IdGrade!=null)
                                            .ToList();


                List<Ens3NotificationEvent> listUnsubmitedEvent = new List<Ens3NotificationEvent>();
                var endDate = startdate;
                foreach (var itemUser in listIdUser)
                {
                    var index = listIdUser.IndexOf(itemUser) + 1;
                    _logger.LogWarning($"Loading...{index}/{listIdUser.Count()} >> {endDate} | {_datetime.ServerTime.ToString()}");
                    endDate = _datetime.ServerTime.ToString();

                    var GetUnsubmittedAttendanceV2Service = await _attendanceService.GetUnsubmittedAttendanceEventV2(new GetUnsubmittedAttendanceEventV2Request
                    {
                        idAcademicYear = idAcademicYear,
                        idSchool = IdSchool,
                        idUser = itemUser.Id
                    });

                    var GetUnsubmittedAttendanceV2 = GetUnsubmittedAttendanceV2Service.IsSuccess ? GetUnsubmittedAttendanceV2Service.Payload : null;

                    if (GetUnsubmittedAttendanceV2 != null)
                    {
                        foreach (var itemUnsubmitted in GetUnsubmittedAttendanceV2)
                        {
                            var listIgGradeByIdEvent = listStudentGradeEvent.Where(e => e.idEvent == itemUnsubmitted.IdEvent).Select(e => e.IdGrade).Distinct().ToList();

                            foreach(var itemGrade in listIgGradeByIdEvent)
                            {
                                Ens3NotificationEvent newItemUnsubmitted = new Ens3NotificationEvent
                                {
                                    IdEvent = itemUnsubmitted.IdEvent,
                                    EventName = itemUnsubmitted.EventName,
                                    StartDatetime = itemUnsubmitted.StartDate,
                                    EndDatetime = itemUnsubmitted.EndDate,
                                    AttendanceCheckName = itemUnsubmitted.AttendanceCheckName,
                                    AttendanceTimeSpan = itemUnsubmitted.AttendanceTime,
                                    IdTeacher = itemUser.Id,
                                    Teacher = itemUser.DisplayName,
                                    IdGrade = itemGrade,
                                };

                                listUnsubmitedEvent.Add(newItemUnsubmitted);
                            }
                        }
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
                    var param = new GetSubjectByUserRequest
                    {
                        IdAcademicYear = idAcademicYear,
                        IdUser = idUser,
                        ListIdTeacherPositions = listTeacherPosition,
                    };

                    var GetSubjectByUserService = await _rolePositionService.GetSubjectByUser(new GetSubjectByUserRequest
                    {
                        IdAcademicYear = idAcademicYear,
                        IdUser = idUser,
                        ListIdTeacherPositions = listTeacherPosition,
                    });

                    var GetSubjectByUser = GetSubjectByUserService.IsSuccess ? GetSubjectByUserService.Payload : null;

                    if (GetSubjectByUser != null)
                    {
                        var listIdGrade = GetSubjectByUser
                                            .Select(e => e.Grade.Id).Distinct()
                                            .ToList();

                        var listUnsubmitedByUserPosition = listUnsubmitedEvent
                                                            .Where(e => listIdGrade.Contains(e.IdGrade))
                                                            .GroupBy(e => new
                                                            {
                                                                EventName = e.EventName,
                                                                StartDatetime = e.StartDatetime,
                                                                EndDatetime = e.EndDatetime,
                                                                AttendanceCheckName = e.AttendanceCheckName,
                                                                AttendanceTimeSpan = e.AttendanceTimeSpan,
                                                                IdTeacher = e.IdTeacher,
                                                                Teacher = e.Teacher
                                                            })
                                                            .Select(e => new Ens3NotificationEvent
                                                            {
                                                                EventName = e.Key.EventName,
                                                                StartDate = e.Key.StartDatetime.ToString("dd MMM yyyy | HH:mm"),
                                                                EndDate = e.Key.EndDatetime.ToString("dd MMM yyyy | HH:mm"),
                                                                AttendanceCheckName = e.Key.AttendanceCheckName,
                                                                AttendanceTime = $"{e.Key.AttendanceTimeSpan.Hours}:{e.Key.AttendanceTimeSpan.Minutes}",
                                                                IdTeacher = e.Key.IdTeacher,
                                                                Teacher = e.Key.Teacher
                                                            })
                                                            .OrderBy(e => e.EventName)
                                                            .ToList();

                        if (listUnsubmitedByUserPosition.Any())
                            listAttendanceEmail.Add(new ENS3NotificationRequest
                            {
                                IdUser = idUser,
                                Event = listUnsubmitedByUserPosition
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
                _notificationData["Data"] = itemAttendance.Event;
                _notificationData["dateValid"] = $"{_datetime.ServerTime:dd MMMM yyyy hh:mm tt} (GMT+{DateTimeUtil.OffsetHour})";

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

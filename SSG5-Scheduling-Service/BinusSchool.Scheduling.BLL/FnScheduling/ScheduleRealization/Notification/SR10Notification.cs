using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Api.Scheduling.FnSchedule;
using BinusSchool.Data.Model.Scheduling.FnSchedule.RolePosition;
using BinusSchool.Data.Model.Scheduling.FnSchedule.ScheduleRealization;
using BinusSchool.Data.Model.Scoring.FnScoring.SendEmail.ApprovalByEmail;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using HandlebarsDotNet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SendGrid.Helpers.Mail;

namespace BinusSchool.Scheduling.FnSchedule.ScheduleRealization.Notification
{
    public class SR10Notification : FunctionsNotificationHandler
    {
        private IDictionary<string, object> _notificationData;
        private readonly ILogger<SR10Notification> _logger;
        private readonly ISchedulingDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly IRolePosition _rolePosition;
        private string _schoolName;
        private GetEmailScheduleRealizationResult _EmailScheduleRealization;
        private List<EmailAddress> _listEmailCC;

        public SR10Notification(INotificationManager notificationManager, IRolePosition rolePosition, IConfiguration configuration, ILogger<SR10Notification> logger, ISchedulingDbContext dbContext, IDictionary<string, object> notificationData) :
           base("SR10", notificationManager, configuration, logger)
        {
            _dbContext = dbContext;
            _configuration = configuration;
            _rolePosition = rolePosition;
            _logger = logger;
        }

        protected override Task FetchNotificationConfig()
        {
            NotificationConfig = new NotificationConfig
            {
                EnEmail = true,
                EnPush = new EnablePushConfig
                {
                    Mobile = false,
                    Web = true
                }
            };

            return Task.CompletedTask;
        }

        protected override Task Prepare()
        {
            var Object = KeyValues.FirstOrDefault(e => e.Key == "EmailScheduleRealization").Value;
            _EmailScheduleRealization = JsonConvert.DeserializeObject<GetEmailScheduleRealizationResult>(JsonConvert.SerializeObject(Object));
            var idAcademicYear = _EmailScheduleRealization.IdAcademicYear;

            var settingEmailSchedule = _dbContext.Entity<MsSettingEmailScheduleRealization>()
                .Where(x => x.IdSchool == IdSchool)
                .Select(x => new GetUserEmailRecepient
                {
                    IdRole = x.IdRole,
                    IdTeacherPosition = x.IdTeacherPosition,
                    IdUser = x.IdBinusian
                })
                .ToList();

            var userSettingSchedule = _rolePosition.GetUserSubjectByEmailRecepient(new GetUserSubjectByEmailRecepientRequest
            {
                IdSchool = IdSchool,
                IdAcademicYear = idAcademicYear,
                IsShowIdUser = true,
                EmailRecepients = settingEmailSchedule,
            }).Result;

            var userCCs = userSettingSchedule.Payload;

            _listEmailCC = _dbContext.Entity<MsUser>().Where(x => userCCs.Select(x => x.IdUser).Contains(x.Id))
                .Select(x => new EmailAddress
                {
                    Email = x.Email,
                    Name = x.DisplayName
                }).ToList();

            _schoolName = _dbContext.Entity<MsSchool>().Where(x => x.Id == IdSchool).Select(x => x.Name).FirstOrDefault();

            return Task.CompletedTask;
        }

        protected override async Task SendEmailNotification()
        {
            try
            {
                if (KeyValues is null)
                {
                    _logger.LogInformation($"Skip sending notification. No data");
                    return;
                }

                if (IdUserRecipients is null)
                    _logger.LogInformation($"Skip sending notification. No Id User Recipients");

                var sendEmailTasks = new List<Task>();

                var users = await _dbContext.Entity<MsUser>()
                            .Where(x => IdUserRecipients.Contains(x.Id))
                            .Select(x => new
                            {
                                Id = x.Id,
                                DisplayName = x.DisplayName,
                                EmailAddress = new EmailAddress(x.Email, x.DisplayName)
                            })
                            .ToListAsync(CancellationToken);

                var GetUserTeacher = await _dbContext.Entity<MsUser>()
                .Where(x => _EmailScheduleRealization.IdUserTeacher.Contains(x.Id) && x.Status)
                .Select(x => new
                {
                    x.Id,
                    EmailAddress = new EmailAddress(x.Email, x.DisplayName),
                    Name = x.DisplayName
                })
                .ToListAsync(CancellationToken);

                var GetSubtituteUserTeacher = await _dbContext.Entity<MsUser>()
                .Where(x => _EmailScheduleRealization.IdUserSubtituteTeacher.Contains(x.Id) && x.Status)
                .Select(x => new
                {
                    x.Id,
                    EmailAddress = new EmailAddress(x.Email, x.DisplayName),
                    Name = x.DisplayName
                })
                .ToListAsync(CancellationToken);

                var dataVenue = _dbContext.Entity<MsVenue>();

                foreach (var idUser in IdUserRecipients)
                {

                    if (!EnsureAnyEmails(users.Where(x => x.Id == idUser).Select(x => x.EmailAddress)))
                        continue;

                    var userEmail = users.Where(x => x.Id == idUser).Select(x => new { x.EmailAddress, x.DisplayName }).FirstOrDefault();

                    _notificationData = new Dictionary<string, object>
                    {
                        { "ClassTeacher",userEmail.DisplayName },
                        { "DateRange",_EmailScheduleRealization.Date.Value.ToString("dd MMMM yyyy") },
                        { "ClassID", _EmailScheduleRealization.ClassID},
                        { "SchoolDay", _EmailScheduleRealization.DaysOfWeek},
                        { "Session", $"Session {_EmailScheduleRealization.SessionID} ({_EmailScheduleRealization.SessionStartTime.ToString(@"hh\:mm")} - {_EmailScheduleRealization.SessionEndTime.ToString(@"hh\:mm")})"},
                        { "TeacherName", string.Join(",", GetUserTeacher.Select(x => x.Name).ToList())},
                        { "RegularVenue", dataVenue.Where(x => x.Id == _EmailScheduleRealization.IdRegularVenue).First().Description},
                        { "SchoolName", _schoolName.ToUpper() }
                    };

                    var emailTemplate = Handlebars.Compile(NotificationTemplate.Email);
                    var emailBody = emailTemplate(_notificationData);

                    // NOTE: create SendGridMessage object to send email
                    var message = new SendGridMessage
                    {
                        Subject = NotificationTemplate.Title,
                        Personalizations = new List<Personalization>
                        {
                            new Personalization { Tos = new List<EmailAddress> { userEmail.EmailAddress }, Ccs = _listEmailCC }
                        }
                    };

                    if (NotificationTemplate.EmailIsHtml)
                        message.HtmlContent = emailBody;
                    else
                        message.PlainTextContent = emailBody;

                    sendEmailTasks.Add(NotificationManager.SendEmail(message));

                }
                // send batch email
                await Task.WhenAll(sendEmailTasks);
            }
            catch (Exception ex)
            {

                _logger.LogError(ex, ex.Message);
            }
        }

        protected override async Task SaveNotification(IEnumerable<string> idUserRecipients, bool isBlast)
        {
            try
            {
                if (KeyValues is null)
                {
                    _logger.LogInformation($"Skip sending notification. No data");
                    return;
                }

                if (IdUserRecipients is null)
                    _logger.LogInformation($"Skip sending notification. No Id User Recipients");

                var staff = await _dbContext.Entity<MsUser>()
                    .Where(x => IdUserRecipients.Contains(x.Id))
                    .Select(x => new
                    {
                        Id = x.Id,
                        EmailAddress = new EmailAddress(x.Email, x.DisplayName)
                    })
                    .ToListAsync(CancellationToken);

                var saveNotificationTasks = new List<Task>();
                foreach (var idUser in IdUserRecipients)
                {
                    var staffEmail = staff.Where(x => x.Id == idUser).Select(x => x.EmailAddress).FirstOrDefault();

                    if (!EnsureAnyEmails(staff.Where(x => x.Id == idUser).Select(x => x.EmailAddress)))
                    {
                        GeneratedContent = $"Canceled classes are cancelled. Please check your email for the details.";
                    }
                    else
                    {
                        var pushTemplate = Handlebars.Compile(NotificationTemplate.Push);
                        GeneratedContent = pushTemplate(_notificationData);

                    }
                    var pushTitle = Handlebars.Compile(NotificationTemplate.Title);
                    GeneratedTitle = pushTitle(_notificationData);

                    var idRecepient = new[] { idUser };

                    saveNotificationTasks.Add(NotificationManager.SaveNotification(
                    CreateNotificationHistory(
                        idUserRecipients,
                        isBlast,
                    GeneratedTitle ?? NotificationTemplate.Title,
                    GeneratedContent ?? NotificationTemplate.Push)));
                }
                await Task.WhenAll(saveNotificationTasks);
            }
            catch (Exception ex)
            {

                _logger.LogError(ex, ex.Message);
            }
        }

        protected override Task SendPushNotification()
        {
            return Task.CompletedTask;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.AppointmentBooking;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using FirebaseAdmin.Messaging;
using FluentEmail.Core.Models;
using HandlebarsDotNet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SendGrid.Helpers.Mail;

namespace BinusSchool.Scheduling.FnSchedule.BreakSetting
{
    public class APP11Notification : FunctionsNotificationHandler
    {
        private IDictionary<string, object> _notificationData;
        private readonly ILogger<APP11Notification> _logger;
        private readonly ISchedulingDbContext _dbContext;
        private readonly IConfiguration _configuration;

        public APP11Notification(INotificationManager notificationManager, IConfiguration configuration, ILogger<APP11Notification> logger, ISchedulingDbContext dbContext, IDictionary<string, object> notificationData) :
           base("APP11", notificationManager, configuration, logger)
        {
            _dbContext = dbContext;
            _configuration = configuration;
            _logger = logger;

            PushNotificationData["action"] = "MD_APPROVAL";
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

        protected override Task Prepare()
        {
            try
            {
                var UrlBase = $"{_configuration.GetSection("ClientApp:Web:Host").Get<string>()}";

                _notificationData = new Dictionary<string, object>
                {
                    { "UrlBase", UrlBase },
                };

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);

                return Task.CompletedTask;
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

            var Object = KeyValues.FirstOrDefault(e => e.Key == "RescheduleInvitationBookingEmail").Value;
            var RescheduleInvitationEmail = JsonConvert.DeserializeObject<List<EmailRescheduleResult>>(JsonConvert.SerializeObject(Object));
            var RescheduleInvitationEmailOld = RescheduleInvitationEmail.Where(e => e.Type == "Old").FirstOrDefault();

            var SendPushNotificationTaks = new List<Task>();

            _notificationData["InvitationName"] = RescheduleInvitationEmailOld.InvitationName;
            _notificationData["StudentName"] = RescheduleInvitationEmailOld.StudentName;
            _notificationData["InvitationDate"] = RescheduleInvitationEmailOld.Date;

            var PushTemplate = Handlebars.Compile(NotificationTemplate.Push);
            var PushBody = PushTemplate(_notificationData);

            var TitleTemplate = Handlebars.Compile(NotificationTemplate.Title);
            var TitleBody = TitleTemplate(_notificationData);


            // NOTE: create MulticastMessage object to send push notification
            var message = new MulticastMessage
            {
                Notification = new FirebaseAdmin.Messaging.Notification
                {
                    Title = TitleBody,
                    Body = PushBody
                },
                Tokens = tokens.Select(e => e.FirebaseToken).ToList(),
                Data = (IReadOnlyDictionary<string, string>)PushNotificationData
            };

            GeneratedTitle = TitleBody;
            GeneratedContent = PushBody;

            await Task.WhenAll(SendPushNotificationTaks);
        }

        protected override async Task SaveNotification(IEnumerable<string> idUserRecipients, bool isBlast)
        {
            var Object = KeyValues.FirstOrDefault(e => e.Key == "RescheduleInvitationBookingEmail").Value;
            var RescheduleInvitationEmail = JsonConvert.DeserializeObject<List<EmailRescheduleResult>>(JsonConvert.SerializeObject(Object));
            var RescheduleInvitationEmailOld = RescheduleInvitationEmail.Where(e => e.Type == "Old").FirstOrDefault();

            var saveNotificationTasks = new List<Task>();

            _notificationData["InvitationName"] = RescheduleInvitationEmailOld.InvitationName;
            _notificationData["StudentName"] = RescheduleInvitationEmailOld.StudentName;
            _notificationData["InvitationDate"] = RescheduleInvitationEmailOld.Date;

            var pushTemplate = Handlebars.Compile(NotificationTemplate.Push);
            GeneratedContent = pushTemplate(_notificationData);

            var pushTitle = Handlebars.Compile(NotificationTemplate.Title);
            GeneratedTitle = pushTitle(_notificationData);

            saveNotificationTasks.Add(NotificationManager.SaveNotification(
            CreateNotificationHistory(
                idUserRecipients,
                isBlast,
            GeneratedTitle ?? NotificationTemplate.Title,
            GeneratedContent ?? NotificationTemplate.Push)));
            await Task.WhenAll(saveNotificationTasks);
        }

        protected override async Task SendEmailNotification()
        {
            var User = await _dbContext.Entity<MsUser>()
                .Where(x => IdUserRecipients.Contains(x.Id))
                .Select(x => new
                {
                    x.Id,
                    EmailAddress = new Address(x.Email, x.DisplayName),
                    Name = x.DisplayName,
                })
                .FirstOrDefaultAsync(CancellationToken);

            if (User == null)
                return;

            var sendEmailTasks = new List<Task>();

            var Object = KeyValues.FirstOrDefault(e => e.Key == "RescheduleInvitationBookingEmail").Value;
            var RescheduleInvitationEmail = JsonConvert.DeserializeObject<List<EmailRescheduleResult>>(JsonConvert.SerializeObject(Object));
            var RescheduleInvitationEmailOld = RescheduleInvitationEmail.Where(e => e.Type == "Old").FirstOrDefault();

            _notificationData["TeacherName"] = User.Name;

            _notificationData["AcademicYear"] = RescheduleInvitationEmailOld.AcademicYear;
            _notificationData["InvitationName"] = RescheduleInvitationEmailOld.InvitationName;
            _notificationData["BookedByParent"] = RescheduleInvitationEmailOld.BookByParent;
            _notificationData["BookDate"] = RescheduleInvitationEmailOld.Date;
            _notificationData["BookTime"] = RescheduleInvitationEmailOld.Time;
            _notificationData["TeachersName"] = RescheduleInvitationEmailOld.TeacherName;

            var emailTemplate = Handlebars.Compile(NotificationTemplate.Email);
            var emailBody = emailTemplate(_notificationData);

            var titleTemplate = Handlebars.Compile(NotificationTemplate.Title);
            var titleBody = titleTemplate(_notificationData);

            // NOTE: create SendGridMessage object to send email
            var message = new EmailData
            {
                Subject = titleBody,
                ToAddresses = new List<Address>
                        {
                            User.EmailAddress
                        },
                IsHtml = NotificationTemplate.EmailIsHtml,
                Body = emailBody
            };

            sendEmailTasks.Add(NotificationManager.SendSmtp(message));

            // send batch email
            await Task.WhenAll(sendEmailTasks);
        }
    }
}

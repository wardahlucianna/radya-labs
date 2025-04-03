using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Student.FnStudent.CreativityActivityService;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.Employee;
using BinusSchool.Persistence.StudentDb.Entities.School;
using BinusSchool.Persistence.StudentDb.Entities.User;
using FirebaseAdmin.Messaging;
using FluentEmail.Core.Models;
using HandlebarsDotNet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SendGrid.Helpers.Mail;

namespace BinusSchool.Student.FnStudent.CreativityActivityService
{
    public class CAS7Notification : FunctionsNotificationHandler
    {
        private IDictionary<string, object> _notificationData;
        private readonly ILogger<CAS7Notification> _logger;
        private readonly IStudentDbContext _dbContext;
        private readonly IConfiguration _configuration;

        public CAS7Notification(INotificationManager notificationManager, IConfiguration configuration, ILogger<CAS7Notification> logger, IStudentDbContext dbContext, IDictionary<string, object> notificationData) :
           base("CAS7", notificationManager, configuration, logger)
        {
            _dbContext = dbContext;
            _configuration = configuration;
            _logger = logger;
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
                //belum update link
                var UrlBase = $"{_configuration.GetSection("ClientApp:Web:Host").Get<string>()}/creativityactivityservice/detailexperience";

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

            var Object = KeyValues.FirstOrDefault(e => e.Key == "GetExperienceEmail").Value;
            var GetExperienceEmail = JsonConvert.DeserializeObject<EmailUpdateStatusExperienceResult>(JsonConvert.SerializeObject(Object));

            var SendPushNotificationTaks = new List<Task>();

            _notificationData["CasName"] = GetExperienceEmail.CasAdvisorName;
            _notificationData["ExperienceName"] = GetExperienceEmail.ExperienceName;
            _notificationData["Status"] = GetExperienceEmail.Status;
            PushNotificationData["id"] = GetExperienceEmail.Id;
            PushNotificationData["action"] = "CAS_UPDATE";

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
            var Object = KeyValues.FirstOrDefault(e => e.Key == "GetExperienceEmail").Value;
            var GetExperienceEmail = JsonConvert.DeserializeObject<EmailUpdateStatusExperienceResult>(JsonConvert.SerializeObject(Object));

            var saveNotificationTasks = new List<Task>();

            _notificationData["CasName"] = GetExperienceEmail.CasAdvisorName;
            _notificationData["ExperienceName"] = GetExperienceEmail.ExperienceName;
            _notificationData["Status"] = GetExperienceEmail.Status;

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
                     EmailAddress = new EmailAddress(x.Email, x.DisplayName)
                 })
                 .FirstOrDefaultAsync(CancellationToken);

            if (User == null)
                return;

            var sendEmailTasks = new List<Task>();

            var Object = KeyValues.FirstOrDefault(e => e.Key == "GetExperienceEmail").Value;
            var GetExperienceEmail = JsonConvert.DeserializeObject<EmailUpdateStatusExperienceResult>(JsonConvert.SerializeObject(Object));

            _notificationData["StudentName"] = User.EmailAddress.Name;
            _notificationData["CasAdvisorName"] = GetExperienceEmail.CasAdvisorName;
            _notificationData["ExperienceName"] = GetExperienceEmail.ExperienceName;
            _notificationData["StartDate"] = GetExperienceEmail.StartDate;
            _notificationData["EndDate"] = GetExperienceEmail.EndDate;
            _notificationData["Location"] = GetExperienceEmail.Location;
            _notificationData["Status"] = GetExperienceEmail.Status;
            _notificationData["Link"] = $"{_notificationData["UrlBase"]}?id={GetExperienceEmail.Id}";

            var emailTemplate = Handlebars.Compile(NotificationTemplate.Email);
            var emailBody = emailTemplate(_notificationData);

            var titleTemplate = Handlebars.Compile(NotificationTemplate.Title);
            var titleBody = titleTemplate(_notificationData);

            // NOTE: create SendGridMessage object to send email
            var message = new SendGridMessage
            {
                Subject = titleBody,
                Personalizations = new List<Personalization>
                {
                    new Personalization { Tos = new List<EmailAddress> { User.EmailAddress } }
                }
            };

            if (NotificationTemplate.EmailIsHtml)
                message.HtmlContent = emailBody;
            else
                message.PlainTextContent = emailBody;

            sendEmailTasks.Add(NotificationManager.SendEmail(message));
            // send batch email
            await Task.WhenAll(sendEmailTasks);
        }
    }
}

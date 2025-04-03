using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.User;
using BinusSchool.Persistence.StudentDb.Entities.School;
using FirebaseAdmin.Messaging;
using HandlebarsDotNet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SendGrid.Helpers.Mail;
using Newtonsoft.Json;
using BinusSchool.Data.Model.Student.FnGuidanceCounseling.ReportStudentToGc;

namespace BinusSchool.Student.FnGuidanceCounseling.ReportStudentToGc.Notification
{
    public class GC1Notification : FunctionsNotificationHandler
    {
        private IDictionary<string, object> _notificationData;
        private readonly IStudentDbContext _dbContext;

        public GC1Notification(INotificationManager notificationManager, 
            IConfiguration configuration, 
            ILogger<GC1Notification> logger, 
            IStudentDbContext dbContext, 
            IDictionary<string, object> notificationData) :
    base("GC1", notificationManager, configuration, logger)
        {
            _dbContext = dbContext;
            _notificationData = notificationData;

            PushNotificationData["action"] = "GC_REPORT";
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

        protected async override Task Prepare()
        {
            _notificationData = KeyValues;

            var schoolName = await _dbContext.Entity<MsSchool>()
                                        .Where(e => e.Id == IdSchool
                                                )
                                        .Select(e => e.Name)
                                        .FirstOrDefaultAsync(CancellationToken);

            var Object = KeyValues.FirstOrDefault(e => e.Key == "EmailGc1").Value;
            var listEmailGc1 = JsonConvert.DeserializeObject<List<Gc1Result>>(JsonConvert.SerializeObject(Object));

            _notificationData["Data"] = listEmailGc1;
            _notificationData["SchoolName"] = schoolName;

            var pushTitle = Handlebars.Compile(NotificationTemplate.Title);
            var pushContent = Handlebars.Compile(NotificationTemplate.Push);

            GeneratedTitle = pushTitle(_notificationData);
            GeneratedContent = pushContent(_notificationData);
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

            // NOTE: create MulticastMessage object to send push notification
            var message = new MulticastMessage
            {
                Notification = new FirebaseAdmin.Messaging.Notification
                {
                    Title = GeneratedTitle,
                    Body = GeneratedContent
                },
                Tokens = tokens
            };

            // send push notification
            await NotificationManager.SendPushNotification(message);
        }

        protected override async Task SendEmailNotification()
        {
            var emailTo = await _dbContext.Entity<MsUser>()
                  .Where(x => IdUserRecipients.Contains(x.Id))
                  .Select(x => new EmailAddress(x.Email, x.DisplayName))
                  .ToListAsync();

            if (!EnsureAnyEmails(emailTo))
                return;

            _notificationData["Link"] = "https://bss-webclient-uat.azurewebsites.net/studentgcreport";

            foreach(var recepient in emailTo)
            {
                _notificationData["CounselorName"] = recepient.Name;

                var emailTemplate = Handlebars.Compile(NotificationTemplate.Email);
                var emailBody = emailTemplate(_notificationData);

                // NOTE: create SendGridMessage object to send email
                var message = new SendGridMessage
                {
                    Subject = NotificationTemplate.Title,
                    Personalizations = new List<Personalization>
                    {
                        new Personalization { Tos = emailTo }
                    }
                };
                if (NotificationTemplate.EmailIsHtml)
                    message.HtmlContent = emailBody;
                else
                    message.PlainTextContent = GeneratedContent;

                await NotificationManager.SendEmail(message);
            }
        }
    }
}

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
using BinusSchool.Data.Model.Student.FnGuidanceCounseling.ReportStudentToGc;
using Newtonsoft.Json;

namespace BinusSchool.Student.FnGuidanceCounseling.ReportStudentToGc.Notification
{
    public class GC2Notification : FunctionsNotificationHandler
    {
        private IDictionary<string, object> _notificationData;
        private readonly IStudentDbContext _dbContext;

        public GC2Notification(INotificationManager notificationManager, IConfiguration configuration, ILogger<GC2Notification> logger, IStudentDbContext dbContext, IDictionary<string, object> notificationData) :
    base("GC2", notificationManager, configuration, logger)
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

            var Object = KeyValues.FirstOrDefault(e => e.Key == "EmailGc2").Value;
            var listEmailGc2 = JsonConvert.DeserializeObject<Gc2Result>(JsonConvert.SerializeObject(Object));

            _notificationData["DataOld"] = listEmailGc2.DataOld;
            _notificationData["DataNew"] = listEmailGc2.DataNew;
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

            foreach (var recepient in emailTo)
            {
                _notificationData["CounselorName"] = recepient.Name;

                var titleTemplate = Handlebars.Compile(NotificationTemplate.Title);
                var title = titleTemplate(_notificationData);
                var emailTemplate = Handlebars.Compile(NotificationTemplate.Email);
                var emailBody = emailTemplate(_notificationData);

                // NOTE: create SendGridMessage object to send email
                var message = new SendGridMessage
                {
                    Subject = title,
                    Personalizations = new List<Personalization>
                    {
                        new Personalization { Tos = emailTo }
                    }
                };
                if (NotificationTemplate.EmailIsHtml)
                    message.HtmlContent = emailBody;
                else
                    message.PlainTextContent = NotificationTemplate.Push;


                await NotificationManager.SendEmail(message);
            }


        }
    }
}

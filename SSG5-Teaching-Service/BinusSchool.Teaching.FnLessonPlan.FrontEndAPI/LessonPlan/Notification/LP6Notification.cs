using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.ScheduleRealization;
using BinusSchool.Data.Model.Teaching.FnLessonPlan.LessonPlan;
using BinusSchool.Persistence.TeachingDb.Abstractions;
using BinusSchool.Persistence.TeachingDb.Entities.School;
using BinusSchool.Persistence.TeachingDb.Entities.User;
using FirebaseAdmin.Messaging;
using FluentEmail.Core.Models;
using HandlebarsDotNet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SendGrid.Helpers.Mail;

namespace BinusSchool.Teaching.FnLessonPlan.LessonPlan.Notification
{
    public class LP6Notification : FunctionsNotificationHandler
    {
        private const string _idScenario = "LP6";

        private IDictionary<string, object> _notificationData;

        private readonly ITeachingDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly ILogger<LP4Notification> _logger;
        protected string url, IdUser;

        public LP6Notification(INotificationManager notificationManager, IConfiguration configuration, ILogger<LP4Notification> logger, ITeachingDbContext dbContext) :
        base("LP6", notificationManager, configuration, logger)
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
                var Object = KeyValues.FirstOrDefault(e => e.Key == "EmailDownloadLessonPlanSummary").Value;
                var urlDownload = JsonConvert.DeserializeObject<UrlDownload>(JsonConvert.SerializeObject(Object));

                url = urlDownload.Url;
                IdUser = urlDownload.IdUser.FirstOrDefault();

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {

                _logger.LogError(ex, ex.Message);

                return Task.CompletedTask;
            };
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

                var msSchool = await _dbContext.Entity<MsSchool>().FirstOrDefaultAsync(x => x.Id == IdSchool, CancellationToken);

                foreach (var idUser in IdUserRecipients)
                {

                    if (!EnsureAnyEmails(users.Where(x => x.Id == idUser).Select(x => x.EmailAddress)))
                        continue;

                    string userName = "", schoolName = "";
                    var userEmail = users.Where(x => x.Id == idUser).Select(x => x.EmailAddress).FirstOrDefault();

                    userName = users.Select(x => x.DisplayName).FirstOrDefault();
                    schoolName = msSchool.Name;

                    _notificationData = new Dictionary<string, object>
                    {
                        { "User", userName.TrimStart().TrimEnd() },
                        { "SchoolName", schoolName.ToUpper() }
                    };

                    var emailTemplate = Handlebars.Compile(NotificationTemplate.Email);
                    var emailBody = emailTemplate(_notificationData);

                    // NOTE: create SendGridMessage object to send email
                    var message = new SendGridMessage
                    {
                        Subject = NotificationTemplate.Title,
                        Personalizations = new List<Personalization>
                        {
                            new Personalization { Tos = new List<EmailAddress> { userEmail } }
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

            var Object = KeyValues.FirstOrDefault(e => e.Key == "EmailDownloadLessonPlanSummary").Value;
            var urlDownload = JsonConvert.DeserializeObject<UrlDownload>(JsonConvert.SerializeObject(Object));
        }
    }
}

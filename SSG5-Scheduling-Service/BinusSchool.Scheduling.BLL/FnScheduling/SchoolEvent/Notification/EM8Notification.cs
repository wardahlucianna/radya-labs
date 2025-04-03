using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using FirebaseAdmin.Messaging;
using HandlebarsDotNet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SendGrid.Helpers.Mail;
using FluentEmail.Core.Models;


namespace BinusSchool.Scheduling.FnSchedule.SchoolEvent.Notification
{
    public class EM8Notification : FunctionsNotificationHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        private readonly IConfiguration _configuration;

        private readonly INotificationManager _notificationManager;

        private IDictionary<string, object> _notificationData;

        private string _schoolName;

        public EM8Notification(string idScenario, INotificationManager notificationManager, IConfiguration configuration, 
            ILogger<FunctionsNotificationHandler> logger, ISchedulingDbContext dbContext, IDictionary<string, object> notificationData) : base("EM8", notificationManager, configuration, logger)
        {
            _dbContext = dbContext;
            _notificationData = notificationData;
            _configuration = configuration;
            _notificationManager = notificationManager;
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
            var url = $"{_configuration.GetSection("ClientApp:Web:Host").Get<string>()}/certificatesetting";

            _schoolName = await _dbContext.Entity<MsSchool>()
                .Where(x => x.Id == IdSchool)
                .Select(x => x.Name.ToUpper())
                .FirstOrDefaultAsync(CancellationToken);

            _notificationData = new Dictionary<string, object>(KeyValues) 
            {
                { "Link", url },
                { "SchoolName", _schoolName },
            };

            var pushTitle = Handlebars.Compile(NotificationTemplate.Title);
            var pushContent = Handlebars.Compile(NotificationTemplate.Push);

            GeneratedTitle = pushTitle(_notificationData);
            GeneratedContent = pushContent(_notificationData);
        }

        protected override async Task SendEmailNotification()
        {
            //get email from id user recepient
            var approver = await _dbContext.Entity<MsUser>()
                .Where(x => IdUserRecipients.Contains(x.Id))
                .Select(x => new
                {
                    EmailAddress = new EmailAddress(x.Email, x.DisplayName)
                })
                .ToListAsync(CancellationToken);

            if (!EnsureAnyEmails(approver.Select(x => x.EmailAddress)))
            {
                return;
            }

            //subject title
            var subject = NotificationTemplate.Title;
            var subjectTemplate = Handlebars.Compile(NotificationTemplate.Title);
            subject = subjectTemplate(_notificationData);

            //template content
            var emailTemplate = Handlebars.Compile(NotificationTemplate.Email);
            var emailBody = emailTemplate(_notificationData);

            var Tos = new List<Address>();

            foreach (var item in approver)
            {
                Tos.Add(new Address
                {
                    EmailAddress = item.EmailAddress.Email,
                    Name = item.EmailAddress.Name
                });
            }

            var message = new EmailData
            {
                IsHtml = true,
                Subject = subject,
                Body = emailBody,
                ToAddresses = Tos
            };

            await _notificationManager.SendSmtp(message);
        }

        protected async override Task SendPushNotification()
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
    }
}

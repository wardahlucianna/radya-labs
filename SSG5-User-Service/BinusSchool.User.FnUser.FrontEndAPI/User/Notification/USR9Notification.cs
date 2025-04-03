using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities;
using BinusSchool.Persistence.UserDb.Entities.School;
using FirebaseAdmin.Messaging;
using FluentEmail.Core.Models;
using HandlebarsDotNet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BinusSchool.User.FnUser.User.Notification
{
    public class USR9Notification : FunctionsNotificationHandler
    {
        private IDictionary<string, object> _notificationData;

        private readonly IUserDbContext _dbContext;

        public USR9Notification(INotificationManager notificationManager, IConfiguration configuration, ILogger<FunctionsNotificationHandler> logger, IUserDbContext dbContext) :
            base("USR9", notificationManager, configuration, logger)
        {
            _dbContext = dbContext;
        }

        protected override Task FetchNotificationConfig()
        {
            // TODO: get config from actual source
            NotificationConfig = new NotificationConfig
            {
                EnEmail = true,
                EnPush = new EnablePushConfig
                {
                    Mobile = false,
                    Web = false
                }
            };

            return Task.CompletedTask;
        }

        protected override async Task Prepare()
        {
            var hostUrl = Configuration.GetSection("ClientApp:Web:Host").Get<string>();
            var schoolName = await _dbContext.Entity<MsSchool>()
                .Where(x => x.Id == IdSchool)
                .Select(x => x.Name.ToUpper())
                .FirstOrDefaultAsync(CancellationToken);

            _notificationData = new Dictionary<string, object>
            {
                { "receiverName", KeyValues["receiverName"] },
                { "email", KeyValues["email"] },
                { "hostUrl", hostUrl },
                { "link", hostUrl + "/auth/changepassword/" + KeyValues["passwordCode"] },
                { "schoolName", schoolName },
            };

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
                Tokens = tokens,
                Data = PushNotificationData as IReadOnlyDictionary<string, string>
            };

            // send push notification
            await NotificationManager.SendPushNotification(message);
        }

        protected override async Task SendEmailNotification()
        {
            var recipients = new List<Address> { new Address(KeyValues["email"] as string, KeyValues["receiverName"] as string) };

            if (!EnsureAnyEmails(recipients))
                return;

            var bccs = new List<Address>();

            //bccs.Add(new Address("theodorus.v@binus.edu","theodorus.v@binus.edu"));
            //bccs.Add(new Address("bsslog.prod@gmail.com","bsslog.prod@gmail.com"));
            // bccs.Add(new Address("group-itdevelopmentschools@binus.edu","group-itdevelopmentschools@binus.edu"));
            bccs.Add(new Address("itdevschool@binus.edu","itdevschool@binus.edu"));

            var emailTitle = Handlebars.Compile(NotificationTemplate.Title);
            var emailBody = Handlebars.Compile(NotificationTemplate.Email);

            var message = new EmailData
            {
                FromAddress = new Address(IdSchool),
                ToAddresses = recipients,
                BccAddresses = bccs,
                Subject = emailTitle(_notificationData),
                Body = emailBody(_notificationData),
                IsHtml = NotificationTemplate.EmailIsHtml
            };

            // send email notification
            await NotificationManager.SendSmtp(message);
        }

        protected override async Task SaveNotification(IEnumerable<string> idUserRecipients, bool isBlast)
        {
            await Task.CompletedTask;
        }
    }
}

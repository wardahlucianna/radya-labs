using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.User.FnCommunication.Message;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities;
using FirebaseAdmin.Messaging;
using HandlebarsDotNet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SendGrid.Helpers.Mail;

namespace BinusSchool.User.FnCommunication.Message.Notification
{
    public class EHN1Notification : FunctionsNotificationHandler
    {
        private IDictionary<string, object> _notificationData;
        private readonly ILogger<EHN1Notification> _logger;
        private readonly IUserDbContext _dbContext;
        private readonly IConfiguration _configuration;

        public EHN1Notification(INotificationManager notificationManager, IConfiguration configuration, ILogger<EHN1Notification> logger, IUserDbContext dbContext, IDictionary<string, object> notificationData) :
           base("EHN1", notificationManager, configuration, logger)
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
                var UrlBase = $"{_configuration.GetSection("ClientApp:Web:Host").Get<string>()}/communication/messagedetailmailinglist";

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

            var Object = KeyValues.FirstOrDefault(e => e.Key == "GetDetailGroupMailingList").Value;
            var DetailMailingList = JsonConvert.DeserializeObject<GetGroupMailingListDetailsResult>(JsonConvert.SerializeObject(Object));

            _notificationData["MemberName"] = DetailMailingList.GroupMembers.FirstOrDefault().Name;
            _notificationData["GroupName"] = DetailMailingList.GroupName;

            var SendPushNotificationTask = new List<Task>();
            var PushTemplate = Handlebars.Compile(NotificationTemplate.Push);
            var PushBody = PushTemplate(_notificationData);

            var TitleTemplate = Handlebars.Compile(NotificationTemplate.Title);
            var TitleBody = TitleTemplate(_notificationData);

            PushNotificationData["id"] = DetailMailingList.Id;
            PushNotificationData["action"] = "MS_MAILING_DETAIL";

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

            await Task.WhenAll(SendPushNotificationTask);
        }

        protected override async Task SaveNotification(IEnumerable<string> idUserRecipients, bool isBlast)
        {
            var Object = KeyValues.FirstOrDefault(e => e.Key == "GetDetailGroupMailingList").Value;
            var DetailMailingList = JsonConvert.DeserializeObject<GetGroupMailingListDetailsResult>(JsonConvert.SerializeObject(Object));

            _notificationData["memberName"] = DetailMailingList.GroupMembers.FirstOrDefault().Name;
            _notificationData["ownerGroup"] = DetailMailingList.OwnerGroup;
            _notificationData["groupName"] = DetailMailingList.GroupName;
            _notificationData["groupDescription"] = DetailMailingList.GroupDescripction;

            var saveNotificationTasks = new List<Task>();

            PushNotificationData["id"] = DetailMailingList.Id;
            PushNotificationData["action"] = "MS_MAILING_DETAIL";

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
                    EmailAddress = new EmailAddress(x.Email, x.DisplayName),
                    Name = x.DisplayName,
                })
                .FirstOrDefaultAsync(CancellationToken);

            if (User == null)
                return;

            var sendEmailTasks = new List<Task>();

            var Object = KeyValues.FirstOrDefault(e => e.Key == "GetDetailGroupMailingList").Value;
            var GetMailingListSettingEmail = JsonConvert.DeserializeObject<GetGroupMailingListDetailsResult>(JsonConvert.SerializeObject(Object));

            var dataUserMember = _dbContext.Entity<MsUser>()
                .Where(x => GetMailingListSettingEmail.IdListMember.Contains(x.Id))
                .Select(x => new
                {
                    EmailAddress = new EmailAddress(x.Email, x.DisplayName)
                })
                .ToList();

            foreach (var sendEmailTo in dataUserMember)
            {

                _notificationData["memberName"] = sendEmailTo.EmailAddress.Name;
                _notificationData["ownerGroup"] = GetMailingListSettingEmail.OwnerGroup;
                _notificationData["groupName"] = GetMailingListSettingEmail.GroupName;
                _notificationData["groupDescription"] = GetMailingListSettingEmail.GroupDescripction;
                _notificationData["link"] = $"{_notificationData["UrlBase"]}?Id={GetMailingListSettingEmail.Id}";


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
                    new Personalization
                    {
                        Tos = new List<EmailAddress>
                        {
                            sendEmailTo.EmailAddress
                        }
                    }
                }
                };

                //message.AddBcc("group-itdevelopmentschools@binus.edu", "group-itdevelopmentschools@binus.edu");
                message.AddBcc("itdevschool@binus.edu", "itdevschool@binus.edu");

                if (NotificationTemplate.EmailIsHtml)
                    message.HtmlContent = emailBody;
                else
                    message.PlainTextContent = emailBody;

                sendEmailTasks.Add(NotificationManager.SendEmail(message));

            }

            // send batch email
            await Task.WhenAll(sendEmailTasks);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.EmailInvitation;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using FirebaseAdmin.Messaging;
using HandlebarsDotNet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SendGrid.Helpers.Mail;

namespace BinusSchool.Scheduling.FnSchedule.EmailInvitation.Notification
{
    public class APP18Notification : FunctionsNotificationHandler
    {
        private IDictionary<string, object> _notificationData;
        private readonly ILogger<APP18Notification> _logger;
        private readonly ISchedulingDbContext _dbContext;
        private readonly IConfiguration _configuration;

        public APP18Notification(INotificationManager notificationManager, IConfiguration configuration, ILogger<APP18Notification> logger, ISchedulingDbContext dbContext, IDictionary<string, object> notificationData) :
           base("APP18", notificationManager, configuration, logger)
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

        protected override async Task Prepare()
        {
            try
            {
                var UrlBase = $"{_configuration.GetSection("ClientApp:Web:Host").Get<string>()}/invitationbookingparent/create2";

                _notificationData = new Dictionary<string, object>
                {
                    { "UrlBase", UrlBase },
                };

                var school = await _dbContext.Entity<MsSchool>().Where(x => x.Id == IdSchool).FirstOrDefaultAsync(CancellationToken);
                if (school != null)
                    _notificationData["SchoolName"] = school.Name.ToUpper();

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

            var Object = KeyValues.FirstOrDefault(e => e.Key == "EmailInvitatin").Value;
            var EmailInvitation = JsonConvert.DeserializeObject<EmailInvitationResult>(JsonConvert.SerializeObject(Object));

            var SendPushNotificationTaks = new List<Task>();

            _notificationData["InvitationName"] = EmailInvitation.InvitationName;
            _notificationData["TeacherName"] = EmailInvitation.Teacher;
            _notificationData["DateInvitation"] = EmailInvitation.DateInvitation;

            var PushTemplate = Handlebars.Compile(NotificationTemplate.Push);
            var PushBody = PushTemplate(_notificationData);

            var TitleTemplate = Handlebars.Compile(NotificationTemplate.Title);
            var TitleBody = TitleTemplate(_notificationData);


            PushNotificationData["action"] = "IB_PARENT";
            PushNotificationData["Id"] = EmailInvitation.IdInvitationBookingSetting;
            PushNotificationData["NameInvitation"] = EmailInvitation.InvitationName;
            PushNotificationData["NameInvitation"] = EmailInvitation.InvitationName;
            PushNotificationData["IsSchedulingSiblingSameTime"] = EmailInvitation.IsSchedulingSameTime.ToString();
            PushNotificationData["IdStudent"] = EmailInvitation.BinusianId;
            PushNotificationData["IdUserTeacher"] = EmailInvitation.IdTeacher;
            PushNotificationData["NameTeacher"] = EmailInvitation.Teacher;


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
            var Object = KeyValues.FirstOrDefault(e => e.Key == "EmailInvitatin").Value;
            var EmailInvitation = JsonConvert.DeserializeObject<EmailInvitationResult>(JsonConvert.SerializeObject(Object));
            var saveNotificationTasks = new List<Task>();

            _notificationData["InvitationName"] = EmailInvitation.InvitationName;
            _notificationData["TeacherName"] = EmailInvitation.Teacher;
            _notificationData["DateInvitation"] = EmailInvitation.DateInvitation;

            PushNotificationData["action"] = "IB_PARENT";
            PushNotificationData["Id"] = EmailInvitation.IdInvitationBookingSetting;
            PushNotificationData["NameInvitation"] = EmailInvitation.InvitationName;
            PushNotificationData["NameInvitation"] = EmailInvitation.InvitationName;
            PushNotificationData["IsSchedulingSiblingSameTime"] = EmailInvitation.IsSchedulingSameTime.ToString();
            PushNotificationData["IdStudent"] = EmailInvitation.BinusianId;
            PushNotificationData["IdUserTeacher"] = EmailInvitation.IdTeacher;
            PushNotificationData["NameTeacher"] = EmailInvitation.Teacher;

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
                    Name = x.DisplayName
                })
                .FirstOrDefaultAsync(CancellationToken);

            if (User == null)
                return;

            var sendEmailTasks = new List<Task>();

            var Object = KeyValues.FirstOrDefault(e => e.Key == "EmailInvitatin").Value;
            var EmailInvitation = JsonConvert.DeserializeObject<EmailInvitationResult>(JsonConvert.SerializeObject(Object));

            _notificationData["ParentName"] = User.Name;

            _notificationData["ReceiverName"] = User.EmailAddress;
            _notificationData["StudentName"] = EmailInvitation.StudentName;
            _notificationData["BinusianId"] = EmailInvitation.BinusianId;
            _notificationData["InvitationName"] = EmailInvitation.InvitationName;
            _notificationData["EarlyBook"] = EmailInvitation.EarlyBook;
            _notificationData["ParentBook"] = EmailInvitation.ParentBook;
            _notificationData["TeacherName"] = EmailInvitation.Teacher;
            _notificationData["Link"] = EmailInvitation.Link;
            _notificationData["IdTeacher"] = EmailInvitation.IdTeacher;
            _notificationData["IdParent"] = EmailInvitation.IdParent;
            _notificationData["Link"] = $"{_notificationData["UrlBase"]}?Id={EmailInvitation.IdInvitationBookingSetting}&NameInvitation={EmailInvitation.InvitationName}&IsSchedulingSiblingSameTime={EmailInvitation.IsSchedulingSameTime}&IdStudent={EmailInvitation.BinusianId}&IdUserTeacher={EmailInvitation.IdTeacher}&NameTeacher={EmailInvitation.Teacher}";

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
                            User.EmailAddress
                        }
                    }
                }
            };
            //message.AddBcc("group-itdevelopmentschools@binus.edu", "group-itdevelopmentschools@binus.edu");
            message.AddBcc("itdevschool@binus.edu", "itdevschool@binus.edu");
            //message.AddBcc("fambi@radyalabs.id", "fambi@radyalabs.id");
            //message.AddBcc("tri@radyalabs.com", "tri@radyalabs.com");

            if (EmailInvitation.IdSchool=="2")
                message.AddBcc("schoolserpong.acop@binus.edu", "schoolserpong.acop@binus.edu");

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

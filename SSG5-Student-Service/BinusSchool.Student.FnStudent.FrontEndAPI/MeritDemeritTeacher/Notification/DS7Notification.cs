using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Student.FnStudent.MeritDemeritTeacher;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.Employee;
using BinusSchool.Persistence.StudentDb.Entities.School;
using BinusSchool.Persistence.StudentDb.Entities.User;
using FirebaseAdmin.Messaging;
using HandlebarsDotNet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SendGrid.Helpers.Mail;

namespace BinusSchool.Student.FnStudent.MeritDemeritTeacher
{
    public class DS7Notification : FunctionsNotificationHandler
    {
        private string _schoolName;
        private IDictionary<string, object> _notificationData;
        private readonly ILogger<DS7Notification> _logger;
        private readonly IStudentDbContext _dbContext;
        private readonly IConfiguration _configuration;

        public DS7Notification(INotificationManager notificationManager, IConfiguration configuration, ILogger<DS7Notification> logger, IStudentDbContext dbContext, IDictionary<string, object> notificationData) :
           base("DS7", notificationManager, configuration, logger)
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
                var UrlBase = $"{_configuration.GetSection("ClientApp:Web:Host").Get<string>()}/disciplinesystem/detailapprovalmeritdemerit?";

                _notificationData = new Dictionary<string, object>
                {
                    { "UrlBase", UrlBase },
                };

                var pushTitle = Handlebars.Compile(NotificationTemplate.Title);
                var pushContent = Handlebars.Compile(NotificationTemplate.Push);

                GeneratedTitle = pushTitle(_notificationData);
                GeneratedContent = pushContent(_notificationData);

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

            var ObjectMeritDemerit = KeyValues.FirstOrDefault(e => e.Key == "GetDemeritStudent").Value;
            var GetMeritDemerit = JsonConvert.DeserializeObject<List<EmailDemeritRequestApprovalResult>>(JsonConvert.SerializeObject(ObjectMeritDemerit));

            var SendPushNotificationTaks = new List<Task>();

            foreach (var item in GetMeritDemerit)
            {
                var GetTokenById = tokens.Where(e => e.IdUser == item.IdUserApproval).Select(e => e.FirebaseToken).ToList();

                if (!GetTokenById.Any())
                    continue;

                var pushTemplate = Handlebars.Compile(NotificationTemplate.Push);
                var TitleTemplate = Handlebars.Compile(NotificationTemplate.Title);

                // NOTE: create MulticastMessage object to send push notification
                var message = new MulticastMessage
                {
                    Notification = new FirebaseAdmin.Messaging.Notification
                    {
                        Title = TitleTemplate(_notificationData),
                        Body = pushTemplate(_notificationData)
                    },
                    Tokens = GetTokenById,
                    Data = (IReadOnlyDictionary<string, string>)PushNotificationData
                };

                GeneratedTitle = TitleTemplate(_notificationData);
                GeneratedContent = pushTemplate(_notificationData);

            }

            await Task.WhenAll(SendPushNotificationTaks);
        }

        protected override async Task SaveNotification(IEnumerable<string> idUserRecipients, bool isBlast)
        {
            var ObjectMeritDemerit = KeyValues.FirstOrDefault(e => e.Key == "GetDemeritStudent").Value;
            var GetMeritDemerit = JsonConvert.DeserializeObject<List<EmailDemeritRequestApprovalResult>>(JsonConvert.SerializeObject(ObjectMeritDemerit));
            var saveNotificationTasks = new List<Task>();
            foreach (var item in GetMeritDemerit)
            {
                var pushTemplate = Handlebars.Compile(NotificationTemplate.Push);
                GeneratedContent = pushTemplate(_notificationData);

                var pushTitle = Handlebars.Compile(NotificationTemplate.Title);
                GeneratedTitle = pushTitle(_notificationData);


                var idRecepient = new[] { item.IdUserApproval };
                saveNotificationTasks.Add(NotificationManager.SaveNotification(
                CreateNotificationHistory(
                    idUserRecipients,
                    isBlast,
                GeneratedTitle ?? NotificationTemplate.Title,
                GeneratedContent ?? NotificationTemplate.Push)));
            }
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
                .ToListAsync(CancellationToken);

            if (!EnsureAnyEmails(User.Select(x => x.EmailAddress)))
                return;

            var sendEmailTasks = new List<Task>();

            var ObjectMeritDemerit = KeyValues.FirstOrDefault(e => e.Key == "GetDemeritStudent").Value;
            var GetMeritDemerit = JsonConvert.DeserializeObject<List<EmailDemeritRequestApprovalResult>>(JsonConvert.SerializeObject(ObjectMeritDemerit));

            // send unique email to each subject teacher
            foreach (var Item in GetMeritDemerit)
            {
                var subjectTeacherEmail = User.FirstOrDefault(x => x.Id == Item.IdUserApproval)?.EmailAddress;
                if (subjectTeacherEmail is null)
                    continue;

                Item.Link = $"{_notificationData["UrlBase"]}id={Item.Id}&category={(int)Item.CategoryEnum}&idAcadYear={Item.IdAcademicYear}&idLevel={Item.IdLevel}&idGrade={Item.IdGrade}&idHomeroomStudent={Item.IdHomeroomStudent}";

                List<EmailDemeritRequestApprovalResult> Data = new List<EmailDemeritRequestApprovalResult>() { Item };

                _notificationData["ReceiverName"] = subjectTeacherEmail.Name;
                _notificationData["SchoolName"] = Item.SchoolName.ToUpper();
                _notificationData["Data"] = Data;

                var emailTemplate = Handlebars.Compile(NotificationTemplate.Email);
                var emailBody = emailTemplate(_notificationData);


                // NOTE: create SendGridMessage object to send email
                var message = new SendGridMessage
                {
                    Subject = NotificationTemplate.Title,
                    Personalizations = new List<Personalization>
                    {
                        new Personalization { Tos = new List<EmailAddress> { subjectTeacherEmail } }
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
    }
}

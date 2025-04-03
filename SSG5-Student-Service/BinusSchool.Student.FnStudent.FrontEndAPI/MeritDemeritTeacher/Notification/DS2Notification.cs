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
    public class DS2Notification : FunctionsNotificationHandler
    {
        private string _schoolName;
        private IDictionary<string, object> _notificationData;
        private readonly ILogger<DS2Notification> _logger;
        private readonly IStudentDbContext _dbContext;
        private readonly IConfiguration _configuration;

        public DS2Notification(INotificationManager notificationManager, IConfiguration configuration, ILogger<DS2Notification> logger, IStudentDbContext dbContext, IDictionary<string, object> notificationData) :
           base("DS2", notificationManager, configuration, logger)
        {
            PushNotificationData["action"] = "MD_DETAIL";

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

        protected override async Task Prepare()
        {
            var UrlBase = $"{_configuration.GetSection("ClientApp:Web:Host").Get<string>()}/disciplinesystem/studentdisciplinedetail?";

            _notificationData = new Dictionary<string, object>
            {
                { "UrlBase", UrlBase },
            };
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

            var ObjectSanction = KeyValues.FirstOrDefault(e => e.Key == "GetSanction").Value;
            var GetSanction = JsonConvert.DeserializeObject<List<EmailSanctionResult>>(JsonConvert.SerializeObject(ObjectSanction));
            var SendPushNotificationTaks = new List<Task>();

            foreach (var item in GetSanction)
            {
                var GetTokenById = tokens.Where(e => e.IdUser == item.IdUser).Select(e => e.FirebaseToken).ToList();

                if (!GetTokenById.Any())
                    continue;

                _notificationData["StudentName"] = item.StudentName;
                _notificationData["BinussianID"] = item.IdStudent;
                _notificationData["SanctionName"] = item.Sanction;

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
            var ObjectSanction = KeyValues.FirstOrDefault(e => e.Key == "GetSanction").Value;
            var GetSanction = JsonConvert.DeserializeObject<List<EmailSanctionResult>>(JsonConvert.SerializeObject(ObjectSanction));
            var saveNotificationTasks = new List<Task>();

            foreach (var item in GetSanction)
            {

                var pushTemplate = Handlebars.Compile(NotificationTemplate.Push);
                GeneratedContent = pushTemplate(_notificationData);

                var pushTitle = Handlebars.Compile(NotificationTemplate.Title);
                GeneratedTitle = pushTitle(_notificationData);


                var idRecepient = new[] { item.IdUser };
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

            var ObjectSanction = KeyValues.FirstOrDefault(e => e.Key == "GetSanction").Value;
            var GetSanction = JsonConvert.DeserializeObject<List<EmailSanctionResult>>(JsonConvert.SerializeObject(ObjectSanction));

            // send unique email to each subject teacher
            foreach (var item in GetSanction)
            {
                var subjectTeacherEmail = User.FirstOrDefault(x => x.Id == item.IdUser)?.EmailAddress;
                if (subjectTeacherEmail is null)
                    continue;

                item.Link = $"{_notificationData["UrlBase"]}idHomeroomStudent={item.IdHomeroomStudent}&idAcadYear={item.IdAcadYear}&idLevel={item.IdLevel}&idGrade={item.IdGrade}&idHomeroom={item.IdHomeroom}&isPoint={item.IsPoint}";

                List<EmailSanctionResult> Data = new List<EmailSanctionResult>() { item };

                _notificationData["ReceiverName"] = subjectTeacherEmail.Name;
                _notificationData["SchoolName"] = item.SchoolName.ToUpper();
                _notificationData["StudentName"] = item.StudentName;
                _notificationData["SanctionName"] = item.Sanction;
                _notificationData["Data"] = Data;

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

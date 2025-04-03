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
using FluentEmail.Core.Models;
using HandlebarsDotNet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SendGrid.Helpers.Mail;

namespace BinusSchool.Student.FnStudent.MeritDemeritTeacher
{
    public class DS3Notification : FunctionsNotificationHandler
    {
        private string _schoolName;
        private IDictionary<string, object> _notificationData;
        private readonly ILogger<DS3Notification> _logger;
        private readonly IStudentDbContext _dbContext;
        private readonly IConfiguration _configuration;

        public DS3Notification(INotificationManager notificationManager, IConfiguration configuration, ILogger<DS3Notification> logger, IStudentDbContext dbContext, IDictionary<string, object> notificationData) :
           base("DS3", notificationManager, configuration, logger)
        {
            _dbContext = dbContext;
            _configuration = configuration;
            _logger = logger;

            PushNotificationData["action"] = "MD_HISTORY";
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
            var UrlBase = $"{_configuration.GetSection("ClientApp:Web:Host").Get<string>()}/disciplinesystem/detailapprovalmeritdemerit?";

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
                .Select(x => x.FirebaseToken)
                .ToListAsync(CancellationToken);

            if (!EnsureAnyPushTokens(tokens))
                return;

            // NOTE: create MulticastMessage object to send push notification

            var ObjectDemeritStudent = KeyValues.FirstOrDefault(e => e.Key == "GetDemeritStudent").Value;
            var GetDemeritStudent = JsonConvert.DeserializeObject<List<EmailMeritDemeritApprovalResult>>(JsonConvert.SerializeObject(ObjectDemeritStudent));
            foreach (var item in GetDemeritStudent)
            {
                _notificationData["RequestDate"] = item.CreateDate;
                _notificationData["RequestDate"] = item.CreateDate;
                _notificationData["Category"] = item.Category;

                var pushTemplate = Handlebars.Compile(NotificationTemplate.Push);
                var TitleTemplate = Handlebars.Compile(NotificationTemplate.Title);

                var message = new MulticastMessage
                {
                    Notification = new FirebaseAdmin.Messaging.Notification
                    {
                        Title = TitleTemplate(_notificationData),
                        Body = pushTemplate(_notificationData)
                    },
                    Tokens = tokens,
                    Data = (IReadOnlyDictionary<string, string>)PushNotificationData
                };

                GeneratedTitle = TitleTemplate(_notificationData);
                GeneratedContent = pushTemplate(_notificationData);

                await NotificationManager.SendPushNotification(message);
            }
        }

        protected override async Task SaveNotification(IEnumerable<string> idUserRecipients, bool isBlast)
        {
            var saveNotificationTasks = new List<Task>();
            var ObjectDemeritStudent = KeyValues.FirstOrDefault(e => e.Key == "GetDemeritStudent").Value;
            var GetDemeritStudent = JsonConvert.DeserializeObject<List<EmailMeritDemeritApprovalResult>>(JsonConvert.SerializeObject(ObjectDemeritStudent));
            foreach (var item in GetDemeritStudent)
            {
               
                var pushTemplate = Handlebars.Compile(NotificationTemplate.Push);
                GeneratedContent = pushTemplate(_notificationData);

                var pushTitle = Handlebars.Compile(NotificationTemplate.Title);
                GeneratedTitle = pushTitle(_notificationData);


                var idRecepient = new[] { item.TeacherId };
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
                    EmailAddress = new Address(x.Email, x.DisplayName)
                })
                .ToListAsync(CancellationToken);

            if (!EnsureAnyEmails(User.Select(x => x.EmailAddress)))
                return;

            var sendEmailTasks = new List<Task>();

            var ObjectMeritDemerit = KeyValues.FirstOrDefault(e => e.Key == "GetDemeritStudent").Value;
            var GetMeritDemerit = JsonConvert.DeserializeObject<List<EmailMeritDemeritApprovalResult>>(JsonConvert.SerializeObject(ObjectMeritDemerit));

            // send unique email to each subject teacher
            foreach (var Item in GetMeritDemerit)
            {
                var subjectTeacherEmail = User.Where(x => x.Id == Item.TeacherId).Select(e=>e.EmailAddress).ToList();
                if (!subjectTeacherEmail.Any())
                    continue;

                Item.Link = $"{_notificationData["UrlBase"]}id={Item.Id}&category={(int)Item.CategoryEnum}&idAcadYear={Item.IdAcademicYear}&idLevel={Item.IdLevel}&idGrade={Item.IdGrade}&idHomeroomStudent={Item.IdHomeroomStudent}";

                List<EmailMeritDemeritApprovalResult> Data = new List<EmailMeritDemeritApprovalResult>() { Item };

                _notificationData["ReceiverName"] = Item.TeacherName;
                _notificationData["SchoolName"] = Item.SchoolName.ToUpper();
                _notificationData["RequestDate"] = Item.CreateDate;
                _notificationData["Category"] = Item.Category;
                _notificationData["Data"] = Data;

                var TitleTemplate = Handlebars.Compile(NotificationTemplate.Title);
                var EmailTemplate = Handlebars.Compile(NotificationTemplate.Email);

                // NOTE: create SendGridMessage object to send email
                var message = new EmailData
                {
                    Subject = TitleTemplate(_notificationData),
                    ToAddresses = subjectTeacherEmail,
                    IsHtml = NotificationTemplate.EmailIsHtml,
                    Body = EmailTemplate(_notificationData)
                };

                sendEmailTasks.Add(NotificationManager.SendSmtp(message));
            }

            // send batch email
            await Task.WhenAll(sendEmailTasks);
        }
    }
}

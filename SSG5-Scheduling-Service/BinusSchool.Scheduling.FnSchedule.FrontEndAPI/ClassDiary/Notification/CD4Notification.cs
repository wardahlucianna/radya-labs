using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Api.User.FnUser;
using BinusSchool.Data.Model.Scheduling.FnSchedule.AppointmentBooking;
using BinusSchool.Data.Model.Scheduling.FnSchedule.ClassDiary;
using BinusSchool.Data.Model.User.FnUser.Register;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using FirebaseAdmin.Messaging;
using FluentEmail.Core.Models;
using Google.Apis.Util;
using HandlebarsDotNet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Org.BouncyCastle.Utilities.Encoders;
using SendGrid.Helpers.Mail;

namespace BinusSchool.Scheduling.FnSchedule.ClassDiary.Notification
{
    public class CD4Notification : FunctionsNotificationHandler
    {
        private string _urlBase;
        private IDictionary<string, object> _notificationData;
        private readonly ISchedulingDbContext _dbContext;
        private readonly IRegister _registerService;
        private readonly IConfiguration _configuration;

        public CD4Notification(INotificationManager notificationManager, IConfiguration configuration, ILogger<CD4Notification> logger, ISchedulingDbContext dbContext, IDictionary<string, object> notificationData, IRegister registerService) :
    base("CD4", notificationManager, configuration, logger)
        {
            _dbContext = dbContext;
            _notificationData = notificationData;
            _registerService = registerService;
            _configuration = configuration;
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
            _urlBase = $"{_configuration.GetSection("ClientApp:Web:Host").Get<string>()}/classdiary/detailclassdiary";
            _notificationData = new Dictionary<string, object> { };
            var objectOld = KeyValues.FirstOrDefault(e => e.Key == "OldData").Value;
            var _oldData = JsonConvert.DeserializeObject<CD4NotificationResult>(JsonConvert.SerializeObject(objectOld));

            var objectNew = KeyValues.FirstOrDefault(e => e.Key == "NewData").Value;
            var _newData = JsonConvert.DeserializeObject<CD4NotificationResult>(JsonConvert.SerializeObject(objectNew));

            _notificationData.Add("OldAcademicYear", _oldData.AcademicYear);
            _notificationData.Add("OldHomeroom", _oldData.Homeroom);
            _notificationData.Add("OldClassId", _oldData.ClassId);
            _notificationData.Add("OldDate", _oldData.Date);
            _notificationData.Add("OldType", _oldData.Type);
            _notificationData.Add("OldTopic", _oldData.Topic);
            _notificationData.Add("OldRequestDate", _oldData.RequestDate);
            _notificationData.Add("OldStatusApproval", _oldData.StatusApproval);

            _notificationData.Add("AcademicYear", _newData.AcademicYear);
            _notificationData.Add("NewHomeroom", _newData.Homeroom);
            _notificationData.Add("ClassId", _newData.ClassId);
            _notificationData.Add("Date", _newData.Date);
            _notificationData.Add("Type", _newData.Type);
            _notificationData.Add("Topic", _newData.Topic);
            _notificationData.Add("RequestDate", _newData.RequestDate);
            _notificationData.Add("StatusApproval", _newData.StatusApproval);

            _notificationData.Add("ClassDiaryEditor", KeyValues["ClassDiaryEditor"].ToString());
            _notificationData.Add("EditDate", KeyValues["EditDate"].ToString());
            _notificationData.Add("SchoolName", KeyValues["SchoolName"].ToString());

            var pushTitle = Handlebars.Compile(NotificationTemplate.Title);
            var pushContent = Handlebars.Compile(NotificationTemplate.Push);

            GeneratedTitle = pushTitle(_notificationData);
            GeneratedContent = pushContent(_notificationData);

            PushNotificationData["action"] = "CD_DETAIL";
            PushNotificationData["Id"] = KeyValues["IdClassDiary"].ToString();

            return Task.CompletedTask;
        }

        protected override async Task SendPushNotification()
        {
            var paramToken = new GetFirebaseTokenRequest
            {
                IdUserRecipient = IdUserRecipients.ToList(),
            };

            var apiGetToken = await _registerService.GetFirebaseToken(paramToken);
            var getFirebaseToken = apiGetToken.IsSuccess ? apiGetToken.Payload : null;
            if (getFirebaseToken == null)
                return;

            var tokens = getFirebaseToken.Select(e => e.Token).ToList();
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
                Data = (IReadOnlyDictionary<string, string>)PushNotificationData
            };

            // send push notification
            await NotificationManager.SendPushNotification(message);
        }

        protected override async Task SendEmailNotification()
        {
            var listIdUserStudent = await _dbContext.Entity<MsHomeroomStudent>()
               .Where(x => IdUserRecipients.Contains(x.IdStudent))
               .Select(x => x.IdStudent)
               .Distinct()
               .ToListAsync(CancellationToken);

            var Users = await _dbContext.Entity<MsUser>()
               .Where(x => IdUserRecipients.Contains(x.Id))
               .Select(x => new
               {
                   x.Id,
                   EmailAddress = new EmailAddress(x.Email, x.DisplayName),
                   Name = x.DisplayName,
                   IsStudent = listIdUserStudent.Any(e => e == x.Id)
               })
               .ToListAsync(CancellationToken);

            var sendEmailTasks = new List<Task>();
            foreach (var User in Users)
            {
                if (_notificationData.ContainsKey("StudentName"))
                    _notificationData.Remove("StudentName");

                if (_notificationData.ContainsKey("Url"))
                    _notificationData.Remove("Url");

                _notificationData.Add("StudentName", RecipientUtil.AddMrMsRecipients(User.Name, User.IsStudent));
                _notificationData.Add("Url", $"{_urlBase}?id={PushNotificationData["Id"]}");

                var emailTemplate = Handlebars.Compile(NotificationTemplate.Email);
                var emailBody = emailTemplate(_notificationData);

                // NOTE: create SendGridMessage object to send email
                var message = new SendGridMessage
                {
                    Subject = GeneratedTitle,
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

                if (NotificationTemplate.EmailIsHtml)
                    message.HtmlContent = emailBody;
                else
                    message.PlainTextContent = emailBody;

                sendEmailTasks.Add(NotificationManager.SendEmail(message));
            }
            await Task.WhenAll(sendEmailTasks);
        }

        protected override async Task SaveNotification(IEnumerable<string> idUserRecipients, bool isBlast)
        {
            var saveNotificationTasks = new List<Task>();
            saveNotificationTasks.Add(NotificationManager.SaveNotification(
            CreateNotificationHistory(
                idUserRecipients,
                isBlast,
            GeneratedTitle ?? NotificationTemplate.Title,
            GeneratedContent ?? NotificationTemplate.Push)));
            await Task.WhenAll(saveNotificationTasks);
        }


    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.EmergencyAttendance;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.User;
using FirebaseAdmin.Messaging;
using FluentEmail.Core.Models;
using HandlebarsDotNet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SendGrid.Helpers.Mail;

namespace BinusSchool.Student.FnStudent.EmergencyAttendance.Notification
{
    public class ATD13Notification : FunctionsNotificationHandler
    {
        private string _schoolName;
        private List<Atd13Result> _listSubmitedData;
        private IDictionary<string, object> _notificationData;
        private readonly IStudentDbContext _dbContext;

        public ATD13Notification(INotificationManager notificationManager, IConfiguration configuration, ILogger<ATD13Notification> logger, IStudentDbContext dbContext, IDictionary<string, object> notificationData) :
base("ATD13", notificationManager, configuration, logger)
        {
            _dbContext = dbContext;
            _notificationData = notificationData;
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
            var Object = KeyValues.FirstOrDefault(e => e.Key == "ListSubmitedData").Value;
            _listSubmitedData = JsonConvert.DeserializeObject<List<Atd13Result>>(JsonConvert.SerializeObject(Object));


            var titleTemplate = Handlebars.Compile(NotificationTemplate.Title);
            GeneratedTitle = titleTemplate(_notificationData);

            _notificationData = new Dictionary<string, object>
            {

            };

            return Task.CompletedTask;
        }

        protected override async Task SaveNotification(IEnumerable<string> idUserRecipients, bool isBlast)
        {
            var saveNotificationTasks = new List<Task>();
            foreach (var data in _listSubmitedData)
            { 
                _notificationData.Add("StudentName", data.StudentName);

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
            }

            await Task.WhenAll(saveNotificationTasks);
        }

        protected override async Task SendPushNotification()
        {
            var SendPushNotificationTaks = new List<Task>();

            foreach (var data in _listSubmitedData)
            {
                var tokens = await _dbContext.Entity<MsUserPlatform>()
                    .Where(x
                        => x.IdUser == data.IdParent && x.FirebaseToken != null
                        && NotificationConfig.EnPush.AllowedPlatforms.Contains(x.AppPlatform))
                    .Select(x => x.FirebaseToken)
                    .ToListAsync();

                if (!EnsureAnyPushTokens(tokens))
                    continue;

                _notificationData.Add("StudentName", data.StudentName);

                var titleTemplate = Handlebars.Compile(NotificationTemplate.Title);
                var title = titleTemplate(_notificationData);
                var bodyTemplate = Handlebars.Compile(NotificationTemplate.Push);
                var body = titleTemplate(_notificationData);

                var message = new MulticastMessage
                {
                    Notification = new FirebaseAdmin.Messaging.Notification
                    {
                        Title = title,
                        Body = body
                    },
                    Tokens = tokens
                };

                SendPushNotificationTaks.Add(NotificationManager.SendPushNotification(message));
                _notificationData.Remove("StudentName");
            }
            // send batch email
            await Task.WhenAll(SendPushNotificationTaks);
        }
        protected override async Task SendEmailNotification()
        {
            var sendPushNotifTasks = new List<Task>(_listSubmitedData.ToList().Count);
            var idUser = _listSubmitedData.Select(e=>e.IdParent).ToList();
            var listUser = await _dbContext.Entity<MsUser>()
                .Where(x => idUser.Contains(x.Id))
                .Select(x => new
                {
                    x.Id,
                    EmailAddress = new Address(x.Email, x.DisplayName),
                    Name = x.DisplayName
                })
                .ToListAsync(CancellationToken);

            foreach (var data in _listSubmitedData)
            {
                var emailParent = listUser.Where(e => e.Id == data.IdParent).Select(e => e.EmailAddress).FirstOrDefault();

                if (emailParent==null)
                    continue;

                _notificationData.Add("StudentName", data.StudentName);
                _notificationData.Add("SchoolName", _schoolName);

                var titleTemplate = Handlebars.Compile(NotificationTemplate.Title);
                var title = titleTemplate(_notificationData);
                var emailTemplate = Handlebars.Compile(NotificationTemplate.Email);
                var email = titleTemplate(_notificationData);

                var message = new EmailData
                {
                    Subject = title,
                    ToAddresses = new List<Address>() { emailParent },
                    IsHtml = NotificationTemplate.EmailIsHtml,
                };

                if (NotificationTemplate.EmailIsHtml)
                    message.Body = email;
                else
                    message.PlaintextAlternativeBody = NotificationTemplate.Push;


                sendPushNotifTasks.Add(NotificationManager.SendSmtp(message));
                _notificationData.Remove("StudentName");
                _notificationData.Remove("SchoolName");
            }
            // send batch email
            await Task.WhenAll(sendPushNotifTasks);
        }
    }
}

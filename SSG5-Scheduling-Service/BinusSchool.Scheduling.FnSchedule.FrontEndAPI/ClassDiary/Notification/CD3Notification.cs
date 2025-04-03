using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Api.User.FnCommunication;
using BinusSchool.Data.Api.User.FnUser;
using BinusSchool.Data.Model.Scheduling.FnSchedule.AppointmentBooking;
using BinusSchool.Data.Model.Scheduling.FnSchedule.ClassDiary;
using BinusSchool.Data.Model.User.FnUser.Register;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using FirebaseAdmin.Messaging;
using HandlebarsDotNet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SendGrid.Helpers.Mail;

namespace BinusSchool.Scheduling.FnSchedule.ClassDiary.Notification
{
    public class CD3Notification : FunctionsNotificationHandler
    {
        private IDictionary<string, object> _notificationData;
        private readonly ISchedulingDbContext _dbContext;
        private readonly IRegister _registerService;
        private readonly IConfiguration _configuration;
        public CD3Notification(INotificationManager notificationManager, IConfiguration configuration, ILogger<CD3Notification> logger, ISchedulingDbContext dbContext, IDictionary<string, object> notificationData, IRegister registerService) :
    base("CD3", notificationManager, configuration, logger)
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
                EnEmail = false,
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
            _notificationData = KeyValues;
            PushNotificationData["action"] = "CD_DETAIL";
            PushNotificationData["Id"] = KeyValues["IdClassDiary"].ToString();

            var pushTemplate = Handlebars.Compile(NotificationTemplate.Push);
            GeneratedContent = pushTemplate(_notificationData);

            var pushTitle = Handlebars.Compile(NotificationTemplate.Title);
            GeneratedTitle = pushTitle(_notificationData);
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
        protected override Task SendEmailNotification()
        {
            throw new NotImplementedException();
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

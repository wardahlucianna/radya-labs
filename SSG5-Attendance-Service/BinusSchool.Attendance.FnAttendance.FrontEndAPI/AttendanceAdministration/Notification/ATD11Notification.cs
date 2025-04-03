using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceAdministration;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using BinusSchool.Persistence.AttendanceDb.Entities.Student;
using BinusSchool.Persistence.AttendanceDb.Entities.User;
using FirebaseAdmin.Messaging;
using FluentEmail.Core.Models;
using HandlebarsDotNet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SendGrid.Helpers.Mail;

namespace BinusSchool.Attendance.FnAttendance.AttendanceAdministration.Notification
{
    public class ATD11Notification : FunctionsNotificationHandler
    {
        private IDictionary<string, object> _notificationData;
        private readonly IAttendanceDbContext _dbContext;

        public ATD11Notification(INotificationManager notificationManager, IConfiguration configuration, ILogger<ATD11Notification> logger, 
            IAttendanceDbContext dbContext, IDictionary<string, object> notificationData) :
        base("ATD11", notificationManager, configuration, logger)
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
            _notificationData.Add("Data", KeyValues["ListData"] as List<ATD11NotificationModel>);
            _notificationData.Add("SchoolName", KeyValues["SchoolName"] as string);

            var pushTitle = Handlebars.Compile(NotificationTemplate.Title);
            var pushContent = Handlebars.Compile(NotificationTemplate.Push);

            GeneratedTitle = pushTitle(_notificationData);
            GeneratedContent = pushContent(_notificationData);

            return Task.CompletedTask;
        }
        protected override async Task SendPushNotification()
        {
            IReadOnlyList<string> tokens = null;

            var message = new MulticastMessage
            {
                Notification = new FirebaseAdmin.Messaging.Notification
                {
                    Title = GeneratedTitle,
                    Body = GeneratedContent
                },
                Tokens = tokens
            };

            await NotificationManager.SendPushNotification(message);
        }
        protected override async Task SendEmailNotification()
        {
            var emailTemplate = Handlebars.Compile(NotificationTemplate.Email);
            var email = emailTemplate(_notificationData);

            var bccs = new List<Address>();

            var envName = Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT") ??
                Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            if (envName == "Staging")
            {
                //bccs.Add(new Address("bsslog.prod@gmail.com", "bsslog.prod@gmail.com"));
                //bccs.Add(new Address("group-itdevelopmentschools@binus.edu", "group-itdevelopmentschools@binus.edu"));
                bccs.Add(new Address("itdevschool@binus.edu", "itdevschool@binus.edu"));
            }

            var message = new EmailData
            {
                Subject = NotificationTemplate.Title,
                ToAddresses = new List<Address>() { new Address("") },
                IsHtml = NotificationTemplate.EmailIsHtml,
                BccAddresses = bccs
            };

            if (NotificationTemplate.EmailIsHtml)
                message.Body = email;
            else
                message.PlaintextAlternativeBody = NotificationTemplate.Email;

            await NotificationManager.SendSmtp(message);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Persistence.SchedulingDb.Entities.Student;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using FirebaseAdmin.Messaging;
using FluentEmail.Core.Models;
using HandlebarsDotNet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace BinusSchool.Scheduling.FnSchedule.StudentEnrollment.Notification
{
    public class AYS9Notification : FunctionsNotificationHandler
    {
        private string _schoolName;
        private IDictionary<string, object> _notificationData;

        private readonly ISchedulingDbContext _dbContext;
        
        public AYS9Notification(INotificationManager notificationManager, IConfiguration configuration, ILogger<AYS9Notification> logger, ISchedulingDbContext dbContext) : 
            base("AYS9", notificationManager, configuration, logger)
        {
            _dbContext = dbContext;
        }

        protected override Task FetchNotificationConfig()
        {
            // TODO: get config from actual source
            NotificationConfig = new NotificationConfig
            {
                EnEmail = true, // should be false
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
            var hostUrl = Configuration.GetSection("ClientApp:Web:Host").Get<string>();
            
            _schoolName = await _dbContext.Entity<MsSchool>()
                .Where(x => x.Id == IdSchool)
                .Select(x => x.Name.ToUpper())
                .FirstOrDefaultAsync(CancellationToken);
            _notificationData = new Dictionary<string, object>
            {
                { "date", ((DateTime)KeyValues["date"]).ToString("d MMMM yyyy") },
                { "hostUrl", hostUrl },
                { "schoolName", _schoolName },
            };
            
            // remove additional dictionary
            KeyValues.Remove("date");
            
            // update recipients
            IdUserRecipients = KeyValues.Select(x => x.Key);

            var pushTitle = Handlebars.Compile(NotificationTemplate.Title);
            var pushContent = Handlebars.Compile(NotificationTemplate.Push);

            GeneratedTitle = pushTitle(_notificationData);
            GeneratedContent = pushContent(_notificationData);
        }

        protected override async Task SendPushNotification()
        {
            var tokens = await _dbContext.Entity<MsUserPlatform>()
                .Where(x 
                    => KeyValues.Keys.Contains(x.IdUser) && x.FirebaseToken != null
                    && NotificationConfig.EnPush.AllowedPlatforms.Contains(x.AppPlatform))
                .Select(x => x.FirebaseToken)
                .ToListAsync(CancellationToken);
            
            if (!EnsureAnyPushTokens(tokens))
                return;
            
            // create MulticastMessage object to send push notification
            var message = new MulticastMessage
            {
                Notification = new FirebaseAdmin.Messaging.Notification
                {
                    Title = GeneratedTitle,
                    Body = GeneratedContent
                },
                Tokens = tokens
            };

            // send push notification
            await NotificationManager.SendPushNotification(message);
        }

        protected override async Task SendEmailNotification()
        {
            var students = await _dbContext.Entity<MsStudent>()
                .Where(x => KeyValues.Keys.Contains(x.Id))
                .Select(x => new
                {
                    x.Id,
                    EmailAddress = new Address(x.BinusianEmailAddress, NameUtil.GenerateFullName(x.FirstName, x.MiddleName, x.LastName))
                })
                .ToListAsync(CancellationToken);
            
            if (!EnsureAnyEmails(students.Select(x => x.EmailAddress)))
                return;
            
            var sendEmailTasks = new List<Task>(KeyValues.Count);

            // send unique email to each student
            foreach (var student in KeyValues)
            {
                var studentEmail = students.FirstOrDefault(x => x.Id == student.Key)?.EmailAddress;
                if (studentEmail is null)
                    continue;

                var classIds = ((JObject)student.Value).ToObject<IDictionary<string, IEnumerable<string>>>();
                var zipped = classIds!["old"].ZipLongest(classIds["new"], (old, @new) => new { old, @new });
                
                _notificationData["studentName"] = studentEmail.Name;
                _notificationData["classIds"] = zipped;

                var emailTemplate = Handlebars.Compile(NotificationTemplate.Email);
                var emailBody = emailTemplate(_notificationData);

                // create EmailData object to send email
                var message = new EmailData
                {
                    FromAddress = new Address(IdSchool),
                    ToAddresses = new List<Address> { studentEmail },
                    Subject = NotificationTemplate.Title,
                    Body = emailBody,
                    IsHtml = NotificationTemplate.EmailIsHtml
                };

                sendEmailTasks.Add(NotificationManager.SendSmtp(message));
            }

            // send batch email
            await Task.WhenAll(sendEmailTasks);
        }
    }
}

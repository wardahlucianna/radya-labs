using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.Employee;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using FirebaseAdmin.Messaging;
using FluentEmail.Core.Models;
using HandlebarsDotNet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BinusSchool.Scheduling.FnLongRun.BlobTrigger.AscTimetable.Notification
{
    public class ASC1Notification : FunctionsNotificationHandler
    {
        private IDictionary<string, object> _notificationData;

        private readonly ISchedulingDbContext _dbContext;
        
        public ASC1Notification(INotificationManager notificationManager, IConfiguration configuration, ILogger<ASC1Notification> logger, ISchedulingDbContext dbContext) : 
            base("ASC1", notificationManager, configuration, logger)
        {
            _dbContext = dbContext;
        }

        protected override Task FetchNotificationConfig()
        {
            // TODO: get config from actual source
            NotificationConfig = new NotificationConfig
            {
                EnEmail = true,
                EnPush = new EnablePushConfig
                {
                    Mobile = false,
                    Web = true
                }
            };
            
            return Task.CompletedTask;
        }

        protected override async Task Prepare()
        {
            var hostUrl = Configuration.GetSection("ClientApp:Web:Host").Get<string>();
            
            var staffName = await _dbContext.Entity<MsUser>()
                .Where(x => x.Id == IdUserRecipients.First())
                .Select(x => NameUtil.GenerateFullName(x.DisplayName))
                .FirstOrDefaultAsync(CancellationToken);
            var schoolName = await _dbContext.Entity<MsSchool>()
                .Where(x => x.Id == IdSchool)
                .Select(x => x.Name.ToUpper())
                .FirstOrDefaultAsync(CancellationToken);
            
            _notificationData = new Dictionary<string, object>
            {
                { "date", ((DateTime)KeyValues["date"]).ToString("d MMMM yyyy") },
                { "hostUrl", hostUrl },
                { "staffName", staffName },
                { "schoolName", schoolName },
            };

            var pushTitle = Handlebars.Compile(NotificationTemplate.Title);
            var pushContent = Handlebars.Compile(NotificationTemplate.Push);

            GeneratedTitle = pushTitle(_notificationData);
            GeneratedContent = pushContent(_notificationData);
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
            var teamITs = await _dbContext.Entity<MsUser>()
                .Where(x => x.UserRoles.Any(y => y.Role.RoleGroup.Code == RoleConstant.IT))
                .Select(x => x.Id)
                .ToListAsync(CancellationToken);
            
            // fetch email based on idUserRecipients
            var emails = await _dbContext.Entity<MsStaff>()
                .Where(x => IdUserRecipients.Concat(teamITs).Contains(x.IdBinusian) && !string.IsNullOrEmpty(x.BinusianEmailAddress))
                .Select(x => new
                {
                    Id = x.IdBinusian, 
                    Email = new Address(x.BinusianEmailAddress, NameUtil.GenerateFullName(x.FirstName, x.LastName))
                })
                .ToListAsync(CancellationToken);
            
            if (!EnsureAnyEmails(emails.Select(x => x.Email)))
                return;
            
            var emailTemplate = Handlebars.Compile(NotificationTemplate.Email);
            var emailBody = emailTemplate(_notificationData);
                    
            var message = new EmailData
            {
                FromAddress = new Address(IdSchool),
                ToAddresses = new List<Address> { emails.FirstOrDefault(x => x.Id == IdUserRecipients.First())?.Email },
                CcAddresses = emails.GetRange(1, emails.Count - 1).Select(x => x.Email).ToList(),
                Subject = NotificationTemplate.Title,
                Body = emailBody,
                IsHtml = NotificationTemplate.EmailIsHtml
            };

            // send email
            await NotificationManager.SendSmtp(message);
        }
    }
}

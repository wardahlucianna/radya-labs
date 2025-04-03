using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using FluentEmail.Core.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BinusSchool.Scheduling.FnLongRun.BlobTrigger.Scheduling.Notification
{
    public class AYS0Notification : FunctionsNotificationHandler
    {
        private static readonly Lazy<List<string>> _emailTags = new Lazy<List<string>>(new List<string>() { "Generate Schedule" });
        private static readonly Lazy<List<Address>> _emailRecipients = new Lazy<List<Address>>(new List<Address>()
        {
            // new Address("taufik@radyalabs.id"),
            // new Address("cilla.azzahra@radyalabs.id"),
            // new Address("siti@radyalabs.id"),
            // new Address("tri@radyalabs.id"),
            // new Address("mianti.juliansa@binus.edu"),
            // new Address("indra.gunawan@binus.edu")
            new Address("itdevschool@binus.edu")
        });
        
        public AYS0Notification(INotificationManager notificationManager, IConfiguration configuration, ILogger<AYS0Notification> logger) : 
            base("AYS0", notificationManager, configuration, logger, skipGetTemplate: true)
        {
        }

        protected override Task FetchNotificationConfig()
        {
            NotificationConfig = new NotificationConfig
            {
                EnEmail = true
            };
            
            return Task.CompletedTask;
        }

        protected override Task Prepare()
        {
            return Task.CompletedTask;
        }

        protected override Task SendPushNotification()
        {
            return Task.CompletedTask;
        }

        protected override async Task SendEmailNotification()
        {
            KeyValues.TryGetValue("exMessage", out var exMessage);
            KeyValues.TryGetValue("exStackTrace", out var exStackTrace);
            KeyValues.TryGetValue("exInnerMessage", out var exInnerMessage);
            KeyValues.TryGetValue("exInnerStackTrace", out var exInnerStackTrace);
            
            // send error to developer
            var errorMessage = new EmailData
            {
                ToAddresses = _emailRecipients.Value,
                Subject = "An Error Occured, Generated Schedule Failed",
                Body = 
                    $"Message: {exMessage} {Environment.NewLine}Stack Trace: {exStackTrace} {Environment.NewLine}{Environment.NewLine}" +
                    $"Inner Message: {exInnerMessage} {Environment.NewLine}Inner Stack Trace: {exInnerStackTrace}",
                Tags = _emailTags.Value
            };

            await NotificationManager.SendSmtp(errorMessage);
        }

        protected override Task SaveNotification(IEnumerable<string> idUserRecipients, bool isBlast)
        {
            return Task.CompletedTask;
        }
    }
}

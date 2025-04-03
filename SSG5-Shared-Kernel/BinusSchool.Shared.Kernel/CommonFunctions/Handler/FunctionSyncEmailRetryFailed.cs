using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Common.Functions.Repositories;
using FluentEmail.Core.Models;
using HandlebarsDotNet;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Address = FluentEmail.Core.Models.Address;

namespace BinusSchool.Common.Functions.Handler
{
    public static class FunctionSyncEmailRetryFailed
    {
        private const string _failRetryExceedBodyFormat = @"
            Retry already exceed 3 times, with details below (DateTime are in UTC+0):

            Id : {{Id}}
            Domain : {{Domain}}
            FileName : {{FileName}}
            Initial Error Msg : {{Message}}
            First Retry Msg : {{FirstTryMessage}}
            First Retry On : {{FirstTryOn}}
            Second Retry Msg : {{SecondTryMessage}}
            Second Retry On : {{SecondTryOn}}
            Third Retry Msg : {{ThirdTryMessage}}
            Third Retry On : {{ThirdTryOn}}
        ";

        private const string _dateTimeFormat = "yyyy-MM-dd HH:mm:ss";

        public static async Task SendEmailRetryFailedAsync(IServiceScope scope,
            SyncTable item,
            IEnumerable<SyncTableHistory> historiesItems,
            IReadOnlyCollection<string> failRecipients,
            ILogger logger)
        {
            if (failRecipients == null || failRecipients.Count == 0)
                return;

            //only for production or staging
            var env = Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT");
            if (string.IsNullOrWhiteSpace(env) || env.ToUpper() == "DEVELOPMENT")
                return;

            var notificationManager = scope.ServiceProvider.GetRequiredService<INotificationManager>();
            var currentFunctions = scope.ServiceProvider.GetRequiredService<ICurrentFunctions>();

            var mailTemplate = Handlebars.Compile(_failRetryExceedBodyFormat);
            var firstTypeMessage = string.Empty;
            var firstTypeOn = string.Empty;
            var secondTypeMessage = string.Empty;
            var secondTypeOn = string.Empty;
            var thirdTypeMessage = string.Empty;
            var thirdTypeOn = string.Empty;

            var syncTableHistories = historiesItems.ToList();
            if (syncTableHistories.Any())
                for (var i = 0; i < syncTableHistories.Count; i++)
                {
                    switch (i)
                    {
                        case 0:
                            firstTypeMessage = syncTableHistories[i].Message;
                            firstTypeOn = syncTableHistories[i].CreatedDate.ToString(_dateTimeFormat);
                            break;
                        case 1:
                            secondTypeMessage = syncTableHistories[i].Message;
                            secondTypeOn = syncTableHistories[i].CreatedDate.ToString(_dateTimeFormat);
                            break;
                        case 2:
                            thirdTypeMessage = syncTableHistories[i].Message;
                            thirdTypeOn = syncTableHistories[i].CreatedDate.ToString(_dateTimeFormat);
                            break;
                    }
                }

            var data = new Dictionary<string, object>
            {
                { "Id", item.Id },
                { "Domain", item.Domain },
                { "FileName", item.Filename },
                { "Message", item.Message },
                { "FirstTryMessage", firstTypeMessage },
                { "FirstTryOn", firstTypeOn },
                { "SecondTryMessage", secondTypeMessage },
                { "SecondTryOn", secondTypeOn },
                { "ThirdTryMessage", thirdTypeMessage },
                { "ThirdTryOn", thirdTypeOn }
            };
            var subject =
                $"{env} - Retry exceed maximum of ID {item.Id} in {currentFunctions.Domain} service";
            var mailMessage = new EmailData
            {
                ToAddresses = failRecipients.Select(x => new Address(x)).ToList(),
                Subject = subject,
                Body = mailTemplate(data),
                Tags = new List<string>(new[] { "SyncRefTable", "PubSub" })
            };

            await notificationManager.SendSmtp(mailMessage);

            logger.LogInformation("Send email failed retries pub sub is successfully send to fail recipients");
        }
    }
}

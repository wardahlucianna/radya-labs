using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Abstractions;
using FluentEmail.Core.Models;
using HandlebarsDotNet;
using Microsoft.Extensions.DependencyInjection;

namespace BinusSchool.Common.Functions.Handler
{
    public static class FunctionSyncEmail
    {
        private const string _failBodyFormat = @"
            Hub name        : {{pk}}
            Row key         : {{rk}}
            Invocation id   : {{iid}}
            Operation id    : {{oid}}
            Message         : {{msg}}
            Inner message   : {{inmsg}}
            Affect tables   : {{afftd}}
            Value           : {{val}}

            Application insights query:
            union traces
                | union exceptions
                | where timestamp between (datetime(""{{mind}}"") .. datetime(""{{maxd}}""))
                | where operation_Id == '{{oid}}'
                | where customDimensions['InvocationId'] == '{{iid}}'
                | order by timestamp asc
                | project
                    timestamp,
                    message = iff(message != '', message, iff(innermostMessage != '', innermostMessage, customDimensions.['prop__{OriginalFormat}'])),
                    logLevel = customDimensions.['LogLevel']
        ";

        public static async Task SendEmailAsync(IServiceScope scope,
            Dictionary<string, object> data,
            IReadOnlyCollection<string> failRecipients,
            string hubName)
        {
            if (failRecipients == null || failRecipients.Count == 0)
                return;

            //only for production or staging
            var env = Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT");
            if (string.IsNullOrWhiteSpace(env) || env.ToUpper() == "DEVELOPMENT")
                return;

            var notificationManager = scope.ServiceProvider.GetRequiredService<INotificationManager>();
            var currentFunctions = scope.ServiceProvider.GetRequiredService<ICurrentFunctions>();

            var mailTemplate = Handlebars.Compile(_failBodyFormat);
            var mailMessage = new EmailData
            {
                ToAddresses = failRecipients.Select(x => new Address(x)).ToList(),
                Subject =
                    $"{env} - Fail to sync {hubName} in {currentFunctions.Domain} service",
                Body = mailTemplate(data),
                Tags = new List<string>(new[] { "SyncRefTable", "PubSub" })
            };

            await notificationManager.SendSmtp(mailMessage);
        }
    }
}

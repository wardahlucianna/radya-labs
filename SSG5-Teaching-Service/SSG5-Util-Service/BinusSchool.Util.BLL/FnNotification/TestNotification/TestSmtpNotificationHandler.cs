using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using FluentEmail.Core.Models;
using HandlebarsDotNet;
using Microsoft.Extensions.Configuration;
using SendGrid.Helpers.Mail;

namespace BinusSchool.Util.FnNotification.TestNotification
{
    public class TestSmtpNotificationHandler : FunctionsHttpSingleHandler
    {
        private const string _dummyTemplate = @"
            <html>
            <head>
                <title></title>
            </head>
            <body>
                Hello {{name}},
                <br /><br/>
                I'm glad you are trying out the html content feature!
                <br /><br/>
                I hope you are having a great day in {{location.city}} :)
                <br /><br/>
            </body>
            </html>";
        
        private readonly INotificationManager _notificationManager;
        private readonly IConfiguration _configuration;

        public TestSmtpNotificationHandler(INotificationManager notificationManager, IConfiguration configuration)
        {
            _notificationManager = notificationManager;
            _configuration = configuration;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var name = Request.Query["name"].First();
            var city = Request.Query["city"].First();
            var mailTos = Request.Query["mailto"].First().Split(',').Select(x => new Address(x)).ToList();

            var mailTemplate = Handlebars.Compile(_dummyTemplate);
            var mailData = new Dictionary<string, object>
            {
                { "name", name },
                { "location", new { city } },
            };
            var mailContent = mailTemplate(mailData);
            var message = new EmailData
            {
                IsHtml = true,
                FromAddress = new Address
                {
                    EmailAddress = _configuration.GetValue<string>("EmailSender:1:Email"),
                    Name = _configuration.GetValue<string>("EmailSender:1:Name")
                },
                // set dummy subject
                Subject = "Hello World!",
                // set dummy content
                Body = mailContent,
                // set this mail category to Test
                Tags = new[] { "Test" }.ToList(),
                // set recipients
                ToAddresses = mailTos
            };

            var queueMessageId = await _notificationManager.SendSmtp(message);
            
            return Request.CreateApiResult2(queueMessageId as object);
        }
    }
}

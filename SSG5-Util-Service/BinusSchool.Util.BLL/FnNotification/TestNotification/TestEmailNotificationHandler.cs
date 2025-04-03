using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using SendGrid.Helpers.Mail;

namespace BinusSchool.Util.FnNotification.TestNotification
{
    public class TestEmailNotificationHandler : FunctionsHttpSingleHandler
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

        public TestEmailNotificationHandler(INotificationManager notificationManager)
        {
            _notificationManager = notificationManager;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var name = Request.Query["name"].First();
            var city = Request.Query["city"].First();
            var mailTos = Request.Query["mailto"].First().Split(',').Select(x => new EmailAddress(x)).ToList();

            var mailContent = _dummyTemplate.Replace("{{name}}", name).Replace("{{location.city}}", city);
            var message = new SendGridMessage
            {
                // set dummy subject
                Subject = "Hello World!",
                // set dummy content
                HtmlContent = mailContent,
                // set this mail category to Test
                Categories = new[] { "Test" }.ToList()
            };
            message.AddTos(mailTos);

            var queueMessageId = await _notificationManager.SendEmail(message);
            
            return Request.CreateApiResult2(queueMessageId as object);
        }
    }
}
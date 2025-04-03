using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Util.FnNotification.SendGrid;
using BinusSchool.Util.FnNotification.SendGrid.Validator;
using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace BinusSchool.Util.FnNotification.SendGrid
{
    public class AddSendGridDynamicEmailHandler : FunctionsHttpSingleHandler
    {
        private readonly ISendGridClient _sendGridClient;
        private readonly IConfiguration _configuration;

        public AddSendGridDynamicEmailHandler(ISendGridClient sendGridClient, IConfiguration configuration)
        {
            _sendGridClient = sendGridClient;
            _configuration = configuration;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<AddSendGridDynamicEmailRequest, AddSendGridDynamicEmailValidator>();
            var sendGridMessage = new SendGridMessage
            {
                TemplateId = body.IdTemplate,
                From = new EmailAddress(!string.IsNullOrEmpty(body.From) 
                    ? body.From 
                    : _configuration.GetSection("EmailSender:Default:Email").Get<string>()), 
                Personalizations = new List<Personalization>
                {
                    new Personalization
                    {
                        Tos = body.To.Select(x => new EmailAddress(x)).ToList(),
                        TemplateData = body.TemplateData
                    }
                },
                Categories = body.Categories?.ToList() ?? new List<string>()
            };

            if (body.Cc != null && body.Cc.Any())
                sendGridMessage.Personalizations.First()!.Ccs = body.Cc.Select(x => new EmailAddress(x)).ToList();
            if (body.Bcc != null && body.Bcc.Any())
                sendGridMessage.Personalizations.First()!.Bccs = body.Bcc.Select(x => new EmailAddress(x)).ToList();
            sendGridMessage.Categories.Add("Test Send Dynamic");

            var response = await _sendGridClient.SendEmailAsync(sendGridMessage, CancellationToken);
            var responseMessage = response.IsSuccessStatusCode
                ? $"Successfully send dynamic template to {string.Join(", ", body.To!)}"
                : await response.Body.ReadAsStringAsync();

            return Request.CreateApiResult2(responseMessage as object);
        }
    }
}

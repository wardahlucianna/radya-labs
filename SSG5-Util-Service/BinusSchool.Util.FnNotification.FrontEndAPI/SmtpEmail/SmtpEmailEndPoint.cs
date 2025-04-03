using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Data.Model.Util.FnNotification.SmtpEmail;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Util.FnNotification.SmtpEmail
{
    public class SmtpEmailEndPoint
    {
        private const string _route = "smpt-email";
        private const string _tag = "SMTP Email";

        private readonly SendSmtpEmailHandler _sendSmtpEmailHandler;

        public SmtpEmailEndPoint(SendSmtpEmailHandler sendSmtpEmailHandler)
        {
            _sendSmtpEmailHandler = sendSmtpEmailHandler;
        }

        [FunctionName(nameof(SmtpEmailEndPoint.SendSmtpEmail))]
        [OpenApiOperation(tags: _tag, Summary = "Send Email using SMTP")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(SendSmtpEmailRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(SendSmtpEmailResult))]
        public Task<IActionResult> SendSmtpEmail(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/send-email")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _sendSmtpEmailHandler.Execute(req, cancellationToken, false);
        }
    }
}

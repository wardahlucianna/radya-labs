using System.Net;
using System.Threading;
using System.Threading.Tasks;
using FirebaseAdmin.Messaging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Util.FnNotification.TestNotification
{
    public class TestNotificationEndpoint
    {
        private const string _route = "test-notification";
        private const string _tag = "Test Queue Notification";

        [FunctionName(nameof(SendEmailNotification))]
        [OpenApiOperation(tags: _tag, Summary = "Send Test Email Notification")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter("name", In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter("city", In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter("mailto", In = ParameterLocation.Query, Required = true, Type = typeof(string[]))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> SendEmailNotification(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/email")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<TestEmailNotificationHandler>();
            return handler.Execute(req, cancellationToken, false);
        }
        
        [FunctionName(nameof(SendSmtpNotification))]
        [OpenApiOperation(tags: _tag, Summary = "Send Test Smtp Notification")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter("name", In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter("city", In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter("mailto", In = ParameterLocation.Query, Required = true, Type = typeof(string[]))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> SendSmtpNotification(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/smtp")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<TestSmtpNotificationHandler>();
            return handler.Execute(req, cancellationToken, false);
        }

        [FunctionName(nameof(SendTestPushNotification))]
        [OpenApiOperation(tags: _tag, Summary = "Send Test Push Notification")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(MulticastMessage))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> SendTestPushNotification(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/push")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<TestPushNotificationHandler>();
            return handler.Execute(req, cancellationToken, false);
        }
    }
}

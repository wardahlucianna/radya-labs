using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Data.Model.Util.FnNotification.SendGrid;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Util.FnNotification.SendGrid
{
    public class SendGridEndpoint
    {
        private const string _route = "sendgrid";
        private const string _tag = "SendGrid Email";

        [FunctionName(nameof(GetSendGridTemplate))]
        [OpenApiOperation(tags: _tag, Summary = "Get Email Template")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetSendGridTemplateRequest.PageSize), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetSendGridTemplateRequest.PageToken), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSendGridTemplateRequest.Search), In = ParameterLocation.Query)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> GetSendGridTemplate(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/template")] HttpRequest req, 
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetSendGridTemplateHandler>();
            return handler.Execute(req, cancellationToken, false);
        }
        
        [FunctionName(nameof(AddSendGridDynamicEmail))]
        [OpenApiOperation(tags: _tag, Summary = "Send Email with Dynamic Template")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddSendGridDynamicEmailRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> AddSendGridDynamicEmail(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/send-dynamic")] HttpRequest req, 
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<AddSendGridDynamicEmailHandler>();
            return handler.Execute(req, cancellationToken, false);
        }

        [FunctionName(nameof(SendSendGridEmail))]
        [OpenApiOperation(tags: _tag, Summary = "Send Email using Send Grid")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(SendSendGridEmailRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(SendSendGridEmailResult))]
        public Task<IActionResult> SendSendGridEmail(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/send-email")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<SendSendGridEmailHandler>();
            return handler.Execute(req, cancellationToken, false);
        }
    }
}

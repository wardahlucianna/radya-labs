using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Data.Model.Finance.FnPayment.RefundProcessing;
using BinusSchool.Data.Model.Finance.FnPayment.SendEmail;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.SendEmail;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Finance.FnPayment.SendEmail
{
    public class SendEmailEndPoint
    {
        private const string _route = "extracurricular/send-email";
        private const string _tag = "Send Email";

        private readonly SendEmailCancelExtracurricularToParentHandler _sendEmailCancelExtracurricularToParentHandler;
        private readonly SendEmailDeleteNotPaidExtracurricularToParentHandler _sendEmailDeleteNotPaidExtracurricularToParentHandler;
        public SendEmailEndPoint(
            SendEmailCancelExtracurricularToParentHandler sendEmailCancelExtracurricularToParentHandler,
            SendEmailDeleteNotPaidExtracurricularToParentHandler sendEmailDeleteNotPaidExtracurricularToParentHandler)
        {
            _sendEmailCancelExtracurricularToParentHandler = sendEmailCancelExtracurricularToParentHandler;
            _sendEmailDeleteNotPaidExtracurricularToParentHandler = sendEmailDeleteNotPaidExtracurricularToParentHandler;
        }

        [FunctionName(nameof(SendEmailEndPoint.SendEmailCancelledExtracurricularToParent))]
        [OpenApiOperation(tags: _tag, Summary = "Send Email Notification Cancelled Extracurricular To Parent")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(List<SendEmailCancelExtracurricularToParentRequest>))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> SendEmailCancelledExtracurricularToParent(
           [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/email-cancel-parent")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _sendEmailCancelExtracurricularToParentHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(SendEmailEndPoint.SendEmailDeleteNotPaidExtracurricularToParent))]
        [OpenApiOperation(tags: _tag, Summary = "Send Email Notification Delete Not Paid Extracurricular To Parent")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(List<SendEmailDeleteNotPaidExtracurricularToParentRequest>))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> SendEmailDeleteNotPaidExtracurricularToParent(
           [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/email-delete-not-paid-parent")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _sendEmailDeleteNotPaidExtracurricularToParentHandler.Execute(req, cancellationToken);
        }
    }
}

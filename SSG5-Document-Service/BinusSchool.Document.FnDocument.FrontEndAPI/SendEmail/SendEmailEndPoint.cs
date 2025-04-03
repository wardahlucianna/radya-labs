using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Data.Model.Document.FnDocument.SendEmail;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Document.FnDocument.SendEmail
{
    public class SendEmailEndPoint
    {
        private const string _route = "document/send-email";
        private const string _tag = "Send Email";

        private readonly SendEmailClearanceFormForParentHandler _sendEmailClearanceFormForParentHandler;
        private readonly SendEmailClearanceFormForStaffHandler _sendEmailClearanceFormForStaffHandler;
        private readonly SendEmailConcentFormForParentHandler _sendEmailConcentFormForParentHandler;
        private readonly ResendEmailBLPForParentHandler _resendEmailBLPForParentHandler;

        public SendEmailEndPoint(
            SendEmailClearanceFormForParentHandler sendEmailClearanceFormForParentHandler,
            SendEmailClearanceFormForStaffHandler sendEmailClearanceFormForStaffHandler,
            SendEmailConcentFormForParentHandler sendEmailConcentFormForParentHandler,
            ResendEmailBLPForParentHandler resendEmailBLPForParentHandler)
        {
            _sendEmailClearanceFormForParentHandler = sendEmailClearanceFormForParentHandler;
            _sendEmailClearanceFormForStaffHandler = sendEmailClearanceFormForStaffHandler;
            _sendEmailConcentFormForParentHandler = sendEmailConcentFormForParentHandler;
            _resendEmailBLPForParentHandler = resendEmailBLPForParentHandler;
        }

        [FunctionName(nameof(SendEmailEndPoint.SendEmailClearancFormForParent))]
        [OpenApiOperation(tags: _tag, Summary = "Send Email Clearance Form For Parent")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(SendEmailClearanceFormForParentRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> SendEmailClearancFormForParent(
           [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/email-blp-clearance-parent")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _sendEmailClearanceFormForParentHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(SendEmailEndPoint.SendEmailClearancFormForStaff))]
        [OpenApiOperation(tags: _tag, Summary = "Send Email Clearance Form For Staff")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(SendEmailClearanceFormForStaffRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> SendEmailClearancFormForStaff(
           [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/email-blp-clearance-staff")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _sendEmailClearanceFormForStaffHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(SendEmailEndPoint.SendEmailConcentFormForParent))]
        [OpenApiOperation(tags: _tag, Summary = "Send Email Concent Form For Parent (for School Bekasi)")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(SendEmailConcentFormForParentRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> SendEmailConcentFormForParent(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/email-blp-concent-parent")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _sendEmailConcentFormForParentHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(SendEmailEndPoint.ResendEmailBLPForParent))]
        [OpenApiOperation(tags: _tag, Summary = "Resend Email BLP For Parent")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(List<ResendEmailBLPForParentRequest>))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(ResendEmailBLPForParentResult))]
        public Task<IActionResult> ResendEmailBLPForParent(
           [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/resend-email-blp-parent")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _resendEmailBLPForParentHandler.Execute(req, cancellationToken);
        }
    }
}

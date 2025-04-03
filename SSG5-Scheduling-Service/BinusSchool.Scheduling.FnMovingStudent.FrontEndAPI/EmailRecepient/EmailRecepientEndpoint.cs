using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Data.Model.Scheduling.FnMovingStudent.EmailRecepient;
using BinusSchool.Scheduling.FnMovingStudent.EmailRecepient;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Scheduling.FnMovingSubject.EmailRecepient
{
    public class EmailRecepientEndpoint
    {
        private const string _route = "moving-student/email-recepient";
        private const string _tag = "Email Recrpient";

        private readonly EmailRecepientHandler _emailRecepientHandler;
        private readonly GetEmailBccAndTosHandler _getEmailBccAndTosHandler;
        public EmailRecepientEndpoint(EmailRecepientHandler EmailRecepientHandler, GetEmailBccAndTosHandler GetEmailBccAndTosHandler)
        {
            _emailRecepientHandler = EmailRecepientHandler;
            _getEmailBccAndTosHandler = GetEmailBccAndTosHandler;
        }

        [FunctionName(nameof(EmailRecepientEndpoint.GetEmailRecepient))]
        [OpenApiOperation(tags: _tag, Summary = "Get Email Recepient")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetEmailRecepientRequest.Type), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetEmailRecepientRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetEmailRecepientResult[]))]
        public Task<IActionResult> GetEmailRecepient(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route )] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _emailRecepientHandler.Execute(req, cancellationToken);
        }


        [FunctionName(nameof(EmailRecepientEndpoint.AddEmailRecepient))]
        [OpenApiOperation(tags: _tag, Summary = "Add Email Recepient")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddEmailRecepientRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> AddEmailRecepient(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _emailRecepientHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(EmailRecepientEndpoint.GetEmailBccAndTos))]
        [OpenApiOperation(tags: _tag, Summary = "Get Email Recepient")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetEmailBccAndTosRequest.Type), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetEmailBccAndTosRequest[]))]
        public Task<IActionResult> GetEmailBccAndTos(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route+"-to-and-bcc")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _getEmailBccAndTosHandler.Execute(req, cancellationToken);
        }
    }
}

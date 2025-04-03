using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.EmailInvitation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Scheduling.FnSchedule.EmailInvitation
{
    public class EmailInvitationEndpoint
    {
        private const string _route = "schedule/email-invitation";
        private const string _tag = "Email Invitation";

        private readonly EmailInvitationHandler _emailInvitationHandler;
        public EmailInvitationEndpoint(EmailInvitationHandler EmailInvitationHandler)
        {
            _emailInvitationHandler = EmailInvitationHandler;
        }

        [FunctionName(nameof(EmailInvitationEndpoint.GetEmailInvitation))]
        [OpenApiOperation(tags: _tag, Summary = "Get email invitation")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(CollectionRequest.Ids), In = ParameterLocation.Query, Type = typeof(string[]))]
        [OpenApiParameter(nameof(CollectionRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetEmailInvitationRequest.IdInvitationBookingSetting), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetEmailInvitationRequest.StartDate), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetEmailInvitationRequest.EndDate), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetEmailInvitationRequest.IdAcademicYear), In = ParameterLocation.Query,Required =true)]
        [OpenApiParameter(nameof(GetEmailInvitationRequest.IdUser), In = ParameterLocation.Query,Required =true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetEmailInvitationResult))]
        public Task<IActionResult> GetEmailInvitation(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _emailInvitationHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(EmailInvitationEndpoint.AddEmailInvitation))]
        [OpenApiOperation(tags: _tag, Summary = "Create email invitation")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddEmailInvitationRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> AddEmailInvitation(
           [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route )] HttpRequest req,
           [Queue("notification-app")] ICollector<string> collector,
           CancellationToken cancellationToken)
        {
            return _emailInvitationHandler.Execute(req, cancellationToken, keyValues: nameof(collector).WithValue(collector));
        }

        [FunctionName(nameof(EmailInvitationEndpoint.SendEmailInvitation))]
        [OpenApiOperation(tags: _tag, Summary = "Send email invitation")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(SendEmailInvitationRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> SendEmailInvitation(
          [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route)] HttpRequest req,
          [Queue("notification-app")] ICollector<string> collector,
          CancellationToken cancellationToken)
        {
            return _emailInvitationHandler.Execute(req, cancellationToken, keyValues: nameof(collector).WithValue(collector));
        }

        [FunctionName(nameof(EmailInvitationEndpoint.DeleteEmailInvitation))]
        [OpenApiOperation(tags: _tag, Summary = "Delete email invitation")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(IEnumerable<string>))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> DeleteEmailInvitation(
        [HttpTrigger(AuthorizationLevel.Function, "Delete", Route = _route)] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _emailInvitationHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(EmailInvitationEndpoint.GetStudentByEmailInvitation))]
        [OpenApiOperation(tags: _tag, Summary = "Get Student By Email Invitation")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetStudentByEmailInvitationRequest.Role), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetStudentByEmailInvitationRequest.IdInvitationBookingSetting), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetStudentByEmailInvitationRequest.IdParent), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetStudentByEmailInvitationRequest.IdUserTeacher), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetHomeroomStudentResult[]))]
        public Task<IActionResult> GetStudentByEmailInvitation(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-student")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetStudentByEmailInvitationHandler>();
            return handler.Execute(req, cancellationToken);
        }
    }
}

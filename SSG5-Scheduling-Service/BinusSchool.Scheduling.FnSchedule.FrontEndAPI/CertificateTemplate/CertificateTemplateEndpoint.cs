using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.CertificateTemplate;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Scheduling.FnSchedule.CertificateTemplate
{
    public class CertificateTemplateEndpoint
    {
        private const string _route = "schedule/certificate-template";
        private const string _tag = "Certificate Template";

        [FunctionName(nameof(CertificateTemplateEndpoint.GetCertificateTemplate))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(CollectionRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetCertificateTemplateRequest.UserId), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetCertificateTemplateRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetCertificateTemplateRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetCertificateTemplateRequest.ApprovalStatus), In = ParameterLocation.Query, Required = false)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetCertificateTemplateResult))]
        public Task<IActionResult> GetCertificateTemplate(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetCertificateTemplateHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(CertificateTemplateEndpoint.GetCertificateTemplateDetail))]
        [OpenApiOperation(tags: _tag, Summary = "Get Certificate Template Detail")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(DetailCertificateTemplateRequest.IdCertificateTemplate), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(DetailCertificateTemplateRequest.UserId), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(DetailCertificateTemplateResult))]
        public Task<IActionResult> GetCertificateTemplateDetail(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-detail")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<DetailCertificateTemplateHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(CertificateTemplateEndpoint.AddCertificateTemplate))]
        [OpenApiOperation(tags: _tag, Summary = "Add Certificate Template")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddCertificateTempRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> AddCertificateTemplate(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route)] HttpRequest req,
            [Queue("notification-em-schedule")] ICollector<string> collector,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<AddCertificateTemplateHandler>();
            return handler.Execute(req, cancellationToken, keyValues: nameof(collector).WithValue(collector));
        }

        [FunctionName(nameof(CertificateTemplateEndpoint.UpdateCertificateTemplate))]
        [OpenApiOperation(tags: _tag, Summary = "Update Certificate Template")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddCertificateTempRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateCertificateTemplate(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route)] HttpRequest req,
            [Queue("notification-em-schedule")] ICollector<string> collector,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<UpdateCertificateTemplateHandler>();
            return handler.Execute(req, cancellationToken, keyValues: nameof(collector).WithValue(collector));
        }

        [FunctionName(nameof(CertificateTemplateEndpoint.DeleteCertificateTemplate))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Certificate Template")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(DeleteCertificateTemplateRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> DeleteCertificateTemplate(
           [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route + "-delete")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<DeleteCertificateTemplateHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(CertificateTemplateEndpoint.GetCertificateTemplateByAY))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(CollectionRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetCertificateTemplateByAYRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetCertificateTemplateByAYResult))]
        public Task<IActionResult> GetCertificateTemplateByAY(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route +"-by-ay")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetCertificateTemplateByAYHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(CertificateTemplateEndpoint.SetCertificateTemplateApproval))]
        [OpenApiOperation(tags: _tag, Summary = "Set Certificate Template Approval")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(SetCertificateTemplateApprovalRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> SetCertificateTemplateApproval(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route + "-approval")] HttpRequest req,
            [Queue("notification-em-schedule")] ICollector<string> collector,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<SetCertificateTemplateApprovalHandler>();
            return handler.Execute(req, cancellationToken, keyValues: nameof(collector).WithValue(collector));
        }

        [FunctionName(nameof(CertificateTemplateEndpoint.SendEmailApprovalCertificateTempalteNotification))]
        [OpenApiOperation(tags: _tag, Summary = "Send Email Approval Certificate Template Notification")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> SendEmailApprovalCertificateTempalteNotification(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route + "-send-email-notification")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<SendEmailApprovalCertificateTemplateNotificationHandler>();
            return handler.Execute(req, cancellationToken);
        }
    }
}

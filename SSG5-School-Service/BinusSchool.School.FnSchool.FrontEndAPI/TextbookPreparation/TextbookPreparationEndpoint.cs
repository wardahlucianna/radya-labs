using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.School.FnSchool.TextbookPreparation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.School.FnSchool.TextbookPreparation
{
    public class TextbookPreparationEndpoint
    {
        private const string _route = "school/textbook-preparation/entry";
        private const string _tag = "Textbook Preparation";

        private readonly TextbookPreparationHandler _textbookPreparationHandler;
        private readonly UploadTextbookPreparationHandler _uploadTextbookPreparationHandler;
        public TextbookPreparationEndpoint(TextbookPreparationHandler TextbookPreparationHandler, UploadTextbookPreparationHandler UploadTextbookPreparationHandler)
        {
            _textbookPreparationHandler = TextbookPreparationHandler;
            _uploadTextbookPreparationHandler = UploadTextbookPreparationHandler;
        }

        [FunctionName(nameof(TextbookPreparationEndpoint.GetTextbookPreparation))]
        [OpenApiOperation(tags: _tag, Summary = "Get Textbook Preparation")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetTextbookPreparationRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetTextbookPreparationRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetTextbookPreparationRequest.IdLevel), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetTextbookPreparationRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetTextbookPreparationRequest.Status), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetTextbookPreparationRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetTextbookPreparationRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetTextbookPreparationRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetTextbookPreparationRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetTextbookPreparationRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetTextbookPreparationRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetTextbookPreparationResult[]))]
        public Task<IActionResult> GetTextbookPreparation(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetTextBookPreparationHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(TextbookPreparationEndpoint.AddTextbookPreparation))]
        [OpenApiOperation(tags: _tag, Summary = "Add Textbook Preparation")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddTextbookPreparationRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> AddTextbookPreparation(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route)] HttpRequest req,
        [Queue("notification-tp")] ICollector<string> collector,
        CancellationToken cancellationToken)
        {
            return _textbookPreparationHandler.Execute(req, cancellationToken, keyValues: nameof(collector).WithValue(collector));
        }

        [FunctionName(nameof(TextbookPreparationEndpoint.DetailTextbookPreparation))]
        [OpenApiOperation(tags: _tag, Summary = "Detail Textbook Preparation Subject Group")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter("id", Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(DetailTextbookPreparationResult))]
        public Task<IActionResult> DetailTextbookPreparation(
       [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/{id}")] HttpRequest req,
          string id,
       CancellationToken cancellationToken)
        {
            return _textbookPreparationHandler.Execute(req, cancellationToken, true, "id".WithValue(id));
        }

        [FunctionName(nameof(TextbookPreparationEndpoint.UpdateTextbookPreparation))]
        [OpenApiOperation(tags: _tag, Summary = "Update Textbook Preparation")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateTextbookPreparationRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateTextbookPreparation(
        [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route)] HttpRequest req,
        [Queue("notification-tp")] ICollector<string> collector,
        CancellationToken cancellationToken)
        {
            return _textbookPreparationHandler.Execute(req, cancellationToken, keyValues: nameof(collector).WithValue(collector));
        }

        [FunctionName(nameof(TextbookPreparationEndpoint.DeleteTextbookPreparation))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Textbook Preparation")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(IEnumerable<string>), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> DeleteTextbookPreparation(
        [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route)] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _textbookPreparationHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(TextbookPreparationEndpoint.GetTextbookPreparationSubject))]
        [OpenApiOperation(tags: _tag, Summary = "Get Textbook Preparation")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetTextbookPreparationSubjectRequest.IdSubjectGroup), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetTextbookPreparationSubjectRequest.IdLevel), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetTextbookPreparationSubjectRequest.IdGrade), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(NameValueVm[]))]
        public Task<IActionResult> GetTextbookPreparationSubject(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-subject")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetTextBookPreparationSubjectHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(TextbookPreparationEndpoint.GetUploadTextbookPreparation))]
        [OpenApiOperation(tags: _tag, Summary = "Get Upload Textbook Preparation")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetUploadTextbookPreparationResult[]))]
        public Task<IActionResult> GetUploadTextbookPreparation(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route+"-upload")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _uploadTextbookPreparationHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(TextbookPreparationEndpoint.AddUploadTextbookPreparation))]
        [OpenApiOperation(tags: _tag, Summary = "Add Upload Textbook Preparation")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddUploadTextbookPreparationRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> AddUploadTextbookPreparation(
        [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route + "-upload")] HttpRequest req,
        [Queue("notification-tp")] ICollector<string> collector,
        CancellationToken cancellationToken)
        {
            return _uploadTextbookPreparationHandler.Execute(req, cancellationToken, keyValues: nameof(collector).WithValue(collector));
        }

        [FunctionName(nameof(TextbookPreparationEndpoint.DownloadTextbookPreparation))]
        [OpenApiOperation(tags: _tag, Summary = "Download Textbook Preparation")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(DownloadTextbookPreparationRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(DownloadTextbookPreparationRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(DownloadTextbookPreparationRequest.IdLevel), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(DownloadTextbookPreparationRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(DownloadTextbookPreparationRequest.Status), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(DownloadTextbookPreparationRequest.Search), In = ParameterLocation.Query)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> DownloadTextbookPreparation(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-download")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<DownloadTextbookPreparationHandler>();
            return handler.Execute(req, cancellationToken, false);
        }
    }
}

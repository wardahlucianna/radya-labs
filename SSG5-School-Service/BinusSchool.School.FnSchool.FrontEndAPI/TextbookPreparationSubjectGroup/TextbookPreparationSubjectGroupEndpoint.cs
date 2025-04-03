using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Data.Model.School.FnSchool.TextbookPreparationSubjectGroup;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.School.FnSchool.TextbookPreparationSubjectGroup
{
    public class TextbookPreparationSubjectGroupEndpoint
    {
        private const string _route = "school/textbook-preparation/subject-group";
        private const string _tag = "Textbook Preparation Subject Group";

        private readonly TextbookPreparationSubjectGroupHandler _textbookPreparationSubjectGroupHandler;
        public TextbookPreparationSubjectGroupEndpoint(TextbookPreparationSubjectGroupHandler TextbookPreparationSubjectGroupHandler)
        {
            _textbookPreparationSubjectGroupHandler = TextbookPreparationSubjectGroupHandler;
        }

        [FunctionName(nameof(TextbookPreparationSubjectGroupEndpoint.GetTextbookPreparationSubjectGroup))]
        [OpenApiOperation(tags: _tag, Summary = "Get Textbook Preparation Subject Group")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetTextbookPreparationSubjectGroupRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetTextbookPreparationSubjectGroupRequest.IdLevel), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetTextbookPreparationSubjectGroupRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetTextbookPreparationSubjectGroupRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetTextbookPreparationSubjectGroupRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetTextbookPreparationSubjectGroupRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetTextbookPreparationSubjectGroupRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetTextbookPreparationSubjectGroupRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetTextbookPreparationSubjectGroupRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetTextbookPreparationSubjectGroupResult[]))]
        public Task<IActionResult> GetTextbookPreparationSubjectGroup(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _textbookPreparationSubjectGroupHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(TextbookPreparationSubjectGroupEndpoint.GetSubject))]
        [OpenApiOperation(tags: _tag, Summary = "Get Subject")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(GetTextbookPreparationSubjectRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetTextbookPreparationSubjectResult[]))]
        public Task<IActionResult> GetSubject(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route +"-subject")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetTextbookPreparationSubjectHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(TextbookPreparationSubjectGroupEndpoint.AddTextbookPreparationSubjectGroup))]
        [OpenApiOperation(tags: _tag, Summary = "Add Textbook Preparation Subject Group")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddTextbookPreparationSubjectGroupRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> AddTextbookPreparationSubjectGroup(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route)] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _textbookPreparationSubjectGroupHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(TextbookPreparationSubjectGroupEndpoint.DetailTextbookPreparationSubjectGroup))]
        [OpenApiOperation(tags: _tag, Summary = "Detail Textbook Preparation Subject Group")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter("id", Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(DetailTextbookPreparationSubjectGroupResult))]
        public Task<IActionResult> DetailTextbookPreparationSubjectGroup(
       [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/{id}")] HttpRequest req,
          string id,
       CancellationToken cancellationToken)
        {
            return _textbookPreparationSubjectGroupHandler.Execute(req, cancellationToken, true, "id".WithValue(id));
        }

        [FunctionName(nameof(TextbookPreparationSubjectGroupEndpoint.UpdateTextbookPreparationSubjectGroup))]
        [OpenApiOperation(tags: _tag, Summary = "Update Textbook Preparation Subject Group")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateTextbookPreparationSubjectGroupRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateTextbookPreparationSubjectGroup(
        [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route)] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _textbookPreparationSubjectGroupHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(TextbookPreparationSubjectGroupEndpoint.DeleteTextbookPreparationSubjectGroup))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Textbook Preparation Subject Group")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(IEnumerable<string>), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> DeleteTextbookPreparationSubjectGroup(
        [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route)] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _textbookPreparationSubjectGroupHandler.Execute(req, cancellationToken);
        }
    }
}

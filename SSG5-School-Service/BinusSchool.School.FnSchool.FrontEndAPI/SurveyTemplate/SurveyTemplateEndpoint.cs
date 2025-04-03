using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Data.Model;
using BinusSchool.Data.Model.School.FnSchool.SurveyTemplate;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace BinusSchool.School.FnSchool.SurveyTemplate
{
    public class SurveyTemplateEndpoint
    {
        private const string _route = "survey-template";
        private const string _tag = "Survey Template";

        private readonly SurveyTemplateHandler _surveyTemplateHandler;
        private readonly SurveyTemplateCopyHandler _surveyTemplateCopyHandler;
        public SurveyTemplateEndpoint(SurveyTemplateHandler surveyTemplateHandler, SurveyTemplateCopyHandler surveyTemplateCopyHandler)
        {
            _surveyTemplateHandler = surveyTemplateHandler;
            _surveyTemplateCopyHandler = surveyTemplateCopyHandler;
        }

        [FunctionName(nameof(SurveyTemplateEndpoint.GetSurveyTemplate))]
        [OpenApiOperation(tags: _tag, Summary = "Get Survey Template")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(CollectionSchoolRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionSchoolRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionSchoolRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionSchoolRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionSchoolRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionSchoolRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionSchoolRequest.GetAll), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSurveyTemplateRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSurveyTemplateRequest.Type), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetSurveyTemplateResult[]))]
        public Task<IActionResult> GetSurveyTemplate(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _surveyTemplateHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(SurveyTemplateEndpoint.AddSurveyTemplate))]
        [OpenApiOperation(tags: _tag, Summary = "Add Survey Template")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddSurveyTemplateRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> AddSurveyTemplate(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route)] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _surveyTemplateHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(SurveyTemplateEndpoint.DetailSurveyTemplate))]
        [OpenApiOperation(tags: _tag, Summary = "Detail Survey Template")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter("id", Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(DetailSurveyTemplateResult))]
        public Task<IActionResult> DetailSurveyTemplate(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/{id}")] HttpRequest req,
            string id,
            CancellationToken cancellationToken)
        {
            return _surveyTemplateHandler.Execute(req, cancellationToken, true, "id".WithValue(id));
        }

        [FunctionName(nameof(SurveyTemplateEndpoint.UpdateSurveyTemplate))]
        [OpenApiOperation(tags: _tag, Summary = "Update Survey Template")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateSurveyTemplateRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateSurveyTemplate(
       [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route)] HttpRequest req,
       CancellationToken cancellationToken)
        {
            return _surveyTemplateHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(SurveyTemplateEndpoint.DeleteSurveyTemplate))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Survey Template")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(IEnumerable<string>), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> DeleteSurveyTemplate(
        [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route)] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _surveyTemplateHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(SurveyTemplateEndpoint.GetSurveyTemplateCopy))]
        [OpenApiOperation(tags: _tag, Summary = "Get Survey Template Copy")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(CollectionSchoolRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionSchoolRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionSchoolRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionSchoolRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionSchoolRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionSchoolRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionSchoolRequest.GetAll), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSurveyTemplateCopyRequest.IdAcademicYearFrom), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSurveyTemplateCopyRequest.IdAcademicYearTo), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSurveyTemplateCopyRequest.Type), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetSurveyTemplateCopyResult[]))]
        public Task<IActionResult> GetSurveyTemplateCopy(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route+"-copy")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _surveyTemplateCopyHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(SurveyTemplateEndpoint.AddSurveyTemplateCopy))]
        [OpenApiOperation(tags: _tag, Summary = "Add Survey Template Copy")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddSurveyTemplateCopyRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> AddSurveyTemplateCopy(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "-copy")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _surveyTemplateCopyHandler.Execute(req, cancellationToken);
        }
    }
}

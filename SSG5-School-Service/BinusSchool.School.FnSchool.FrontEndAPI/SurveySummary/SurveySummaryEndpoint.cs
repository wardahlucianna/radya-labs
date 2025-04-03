using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Attendance.FnAttendance.PublishSurvey;
using BinusSchool.Common.Extensions;
using BinusSchool.Data.Model;
using BinusSchool.Data.Model.School.FnSchool.SurveySummary;
using BinusSchool.School.FnSchool.SurveySummary;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Attendance.FnAttendance.SurveySummary
{
    public class SurveySummaryEndpoint
    {
        private const string _route = "survey-summary";
        private const string _tag = "Survey Summary";

        private readonly SurveySummaryLogHandler _surveySummaryLogHandler;
        public SurveySummaryEndpoint(SurveySummaryLogHandler surveySummaryLogHandler)
        {
            _surveySummaryLogHandler = surveySummaryLogHandler;
        }

        #region Survey Summary
        [FunctionName(nameof(SurveySummaryEndpoint.GetSurveySummaryUserRespondent))]
        [OpenApiOperation(tags: _tag, Summary = "Get Survey Summary Respondent")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetSurveySummaryUserRespondentRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSurveySummaryUserRespondentRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSurveySummaryUserRespondentRequest.Semester), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSurveySummaryUserRespondentRequest.IdPublishSurvey), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSurveySummaryUserRespondentRequest.IdUser), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetSurveySummaryUserRespondentResult[]))]
        public Task<IActionResult> GetSurveySummaryUserRespondent(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/user-respondent")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetRespondentSurveyHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(SurveySummaryEndpoint.GetSurveySummary))]
        [OpenApiOperation(tags: _tag, Summary = "Get Survey Summary")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(CollectionSchoolRequest.IdSchool), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionSchoolRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionSchoolRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionSchoolRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionSchoolRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionSchoolRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionSchoolRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionSchoolRequest.GetAll), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSurveySummaryRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSurveySummaryRequest.Semester), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetSurveySummaryResult[]))]
        public Task<IActionResult> GetSurveySummary(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetSurveySummaryHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(SurveySummaryEndpoint.GetSurveySummaryRespondent))]
        [OpenApiOperation(tags: _tag, Summary = "Get Survey Summary Respondent")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(CollectionSchoolRequest.IdSchool), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionSchoolRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionSchoolRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionSchoolRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionSchoolRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionSchoolRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionSchoolRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionSchoolRequest.GetAll), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSurveySummaryRespondentRequest.Id), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSurveySummaryRespondentRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetSurveySummaryRespondentResult[]))]
        public Task<IActionResult> GetSurveySummaryRespondent(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/respondent")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetSurveySummaryRespondentHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(SurveySummaryEndpoint.DetailSurveySummaryRespondent))]
        [OpenApiOperation(tags: _tag, Summary = "Detail Survey Summary Respondent")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(DetailSurveySummaryRespondentRequest.IdPublishSurvey), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(DetailSurveySummaryRespondentResult[]))]
        public Task<IActionResult> DetailSurveySummaryRespondent(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/respondent-detail")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<DetailSurveySummaryRespondentHandler>();
            return handler.Execute(req, cancellationToken);
        }
        #endregion

        #region survey Summary Download
        [FunctionName(nameof(SurveySummaryEndpoint.SendEmailSurveySummary))]
        [OpenApiOperation(tags: _tag, Summary = "Send Survey Summary Respondent")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(SendEmailSurveySummaryRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(SendEmailSurveySummaryRequest.IdScenario), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(SendEmailSurveySummaryRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(SendEmailSurveySummaryRequest.Link), In = ParameterLocation.Query)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> SendEmailSurveySummary(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/email")] HttpRequest req,
            [Queue("notification-ess")] ICollector<string> collector,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<SendEmailSurveySummaryHandler>();
            return handler.Execute(req, cancellationToken, keyValues: nameof(collector).WithValue(collector));
        }

        [FunctionName(nameof(SurveySummaryEndpoint.GetSurveySummaryLog))]
        [OpenApiOperation(tags: _tag, Summary = "Get Survey Summary Log")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetSurveySummaryLogRequest.IdUser), In = ParameterLocation.Query,Required =true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetSurveySummaryLogResult))]
        public Task<IActionResult> GetSurveySummaryLog(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/log")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetSurveySummaryLogHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(SurveySummaryEndpoint.AddAndUpdateSurveySummaryLog))]
        [OpenApiOperation(tags: _tag, Summary = "Add And Update Survey Summary Log")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddAndUpdateSurveySummaryLogRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> AddAndUpdateSurveySummaryLog(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/log")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<AddAndUpdateSurveySummaryLogHandler>();
            return handler.Execute(req, cancellationToken);
        }
        #endregion

        [FunctionName(nameof(SurveySummaryEndpoint.GetDownloadSurveySummary))]
        [OpenApiOperation(tags: _tag, Summary = "Download Survey Summary")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(DownloadSurveySummaryRequest.Id), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> GetDownloadSurveySummary(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/download")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<DownloadSurveySummaryHandler>();
            return handler.Execute(req, cancellationToken);
        }
    }
}

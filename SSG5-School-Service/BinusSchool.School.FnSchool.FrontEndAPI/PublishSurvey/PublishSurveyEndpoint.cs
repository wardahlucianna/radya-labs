using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Data.Model;
using BinusSchool.Data.Model.School.FnSchool.PublishSurvey;
using BinusSchool.School.FnSchool.PublishSurvey;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Attendance.FnAttendance.PublishSurvey
{
    public class PublishSurveyEndpoint
    {
        private const string _route = "publish-survey";
        private const string _tag = "Publish Survey";

        private readonly PublishSurveyHandler _publishSurveyHandler;
        public PublishSurveyEndpoint(PublishSurveyHandler publishSurveyHandler)
        {
            _publishSurveyHandler = publishSurveyHandler;
        }

        #region Publish Survey
        [FunctionName(nameof(PublishSurveyEndpoint.GetPublishSurvey))]
        [OpenApiOperation(tags: _tag, Summary = "Get Publish Survey")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(CollectionSchoolRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionSchoolRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionSchoolRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionSchoolRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionSchoolRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionSchoolRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionSchoolRequest.GetAll), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetPublishSurveyRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetPublishSurveyRequest.Semester), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetPublishSurveyRequest.SurveyType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetPublishSurveyRequest.StartDate), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetPublishSurveyRequest.EndDate), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetPublishSurveyRequest.Status), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetPublishSurveyResult[]))]
        public Task<IActionResult> GetPublishSurvey(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _publishSurveyHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(PublishSurveyEndpoint.AddPublishSurvey))]
        [OpenApiOperation(tags: _tag, Summary = "Add Publish Survey")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddPublishSurveyRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(AddPublishSurveyResult))]
        public Task<IActionResult> AddPublishSurvey(
       [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route)] HttpRequest req,
       CancellationToken cancellationToken)
        {
            return _publishSurveyHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(PublishSurveyEndpoint.DetailPublishSurvey))]
        [OpenApiOperation(tags: _tag, Summary = "Detail Publish Survey")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter("id", Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(DetailPublishSurveyResult))]
        public Task<IActionResult> DetailPublishSurvey(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/{id}")] HttpRequest req,
        string id,
            CancellationToken cancellationToken)
        {
            return _publishSurveyHandler.Execute(req, cancellationToken, true, "id".WithValue(id));

        }

        [FunctionName(nameof(PublishSurveyEndpoint.UpdatePublishSurvey))]
        [OpenApiOperation(tags: _tag, Summary = "Update Publish Survey")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdatePublishSurveyRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdatePublishSurvey(
        [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route)] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _publishSurveyHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(PublishSurveyEndpoint.UnpublishSurvey))]
        [OpenApiOperation(tags: _tag, Summary = "Unpublish Survey")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(UnpublishSurveyRequest.Id), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(UnpublishSurveyRequest.IsPublish), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(object[]))]
        public Task<IActionResult> UnpublishSurvey(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-unpublish")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<UnpublishSurveyHandler>();
            return handler.Execute(req, cancellationToken);
        }
        #endregion

        #region Student Learning Survey
        [FunctionName(nameof(PublishSurveyEndpoint.GetResetMappingStudentLearningSurvey))]
        [OpenApiOperation(tags: _tag, Summary = "Get Reset Mapping Student Learning Survey")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetResetMappingStudentLearningSurveyRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetResetMappingStudentLearningSurveyRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetResetMappingStudentLearningSurveyRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetResetMappingStudentLearningSurveyRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetResetMappingStudentLearningSurveyRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetResetMappingStudentLearningSurveyRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetResetMappingStudentLearningSurveyRequest.GetAll), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetResetMappingStudentLearningSurveyRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetResetMappingStudentLearningSurveyRequest.Semester), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetResetMappingStudentLearningSurveyRequest.IdLevel), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetResetMappingStudentLearningSurveyRequest.IdGrade), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetResetMappingStudentLearningSurveyRequest.IdHomeroom), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetResetMappingStudentLearningSurveyRequest.IdPublishSurvey), In = ParameterLocation.Query,Required =true)]
        [OpenApiParameter(nameof(GetResetMappingStudentLearningSurveyRequest.IdStudent), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetResetMappingStudentLearningSurveyResult))]
        public Task<IActionResult> GetResetMappingStudentLearningSurvey(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-reset-mapping-student-leraning")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetResetMappingStudentLearningSurveyHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(PublishSurveyEndpoint.GetMappingStudentLearningSurvey))]
        [OpenApiOperation(tags: _tag, Summary = "Get Mapping Student Learning Survey")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetResetMappingStudentLearningSurveyRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetResetMappingStudentLearningSurveyRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetResetMappingStudentLearningSurveyRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetResetMappingStudentLearningSurveyRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetResetMappingStudentLearningSurveyRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetResetMappingStudentLearningSurveyRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetResetMappingStudentLearningSurveyRequest.GetAll), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetResetMappingStudentLearningSurveyRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetResetMappingStudentLearningSurveyRequest.Semester), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetResetMappingStudentLearningSurveyRequest.IdLevel), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetResetMappingStudentLearningSurveyRequest.IdGrade), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetResetMappingStudentLearningSurveyRequest.IdHomeroom), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetResetMappingStudentLearningSurveyRequest.IdPublishSurvey), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetResetMappingStudentLearningSurveyRequest.IdStudent), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetResetMappingStudentLearningSurveyResult))]
        public Task<IActionResult> GetMappingStudentLearningSurvey(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-mapping-student-leraning")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetMappingStudentLearningSurveyHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(PublishSurveyEndpoint.GetHomeroomMappingStudentLearningSurvey))]
        [OpenApiOperation(tags: _tag, Summary = "Get Homeroom Mapping Student Learning Survey")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetHomeroomMappingStudentLearningSurveyRequest.IdPublishSurvey), In = ParameterLocation.Query,Required =true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetHomeroomMappingStudentLearningSurveyResult))]
        public Task<IActionResult> GetHomeroomMappingStudentLearningSurvey(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-homeroom-mapping-student-leraning")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetHomeroomMappingStudentLearningSurveyHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(PublishSurveyEndpoint.AddMappingStudentLearningSurvey))]
        [OpenApiOperation(tags: _tag, Summary = "Add Mapping Student Learning Survey")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddMappingStudentLearningSurveyRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> AddMappingStudentLearningSurvey(
       [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route+"-mapping")] HttpRequest req,
       CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<AddMappingStudentLearningSurveyHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(PublishSurveyEndpoint.DownloadMappingStudentLearningSurvey))]
        [OpenApiOperation(tags: _tag, Summary = "Download Mapping Student Learning Survey")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(DownloadMappingStudentLearningSurveyRequest.IdPublishSurvey), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> DownloadMappingStudentLearningSurvey(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-download-mapping-student-learning")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<DownloadMappingStudentLearningSurveyHandler>();
            return handler.Execute(req, cancellationToken);
        }
        #endregion

        #region MandatorySurveyUser
        [FunctionName(nameof(PublishSurveyEndpoint.GetMandatorySurveyUser))]
        [OpenApiOperation(tags: _tag, Summary = "Get Mandatory Survey User")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetSurveyMandatoryUserRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSurveyMandatoryUserRequest.Semester), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSurveyMandatoryUserRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSurveyMandatoryUserRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(object[]))]
        public Task<IActionResult> GetMandatorySurveyUser(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-mandatory-survey-user")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetSurveyMandatoryUserHandler>();
            return handler.Execute(req, cancellationToken);
        }
        #endregion
    }
}

using BinusSchool.Common.Extensions;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.School.FnSchool.Pathway;
using BinusSchool.Data.Model.Teaching.FnLessonPlan.LessonPlan;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using System.Collections.Generic;
using System.Net;
using System.Reflection.Metadata;
using System.Threading;
using System.Threading.Tasks;

namespace BinusSchool.Teaching.FnLessonPlan.LessonPlan
{
    public class LessonPlanEndpoint
    {
        private const string _route = "lesson-plan";
        private const string _tag = "Lesson Plan";

        private readonly LessonPlanApproverSettingHandler _lessonPlanApproverSettingHandler;

        public LessonPlanEndpoint(LessonPlanApproverSettingHandler lessonPlanApproverSettingHandler)
        {
            _lessonPlanApproverSettingHandler = lessonPlanApproverSettingHandler;
        }

        [FunctionName(nameof(LessonPlanEndpoint.GetLessonPlanApprovalStatus))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetLessonPlanApprovalStatusRequest.Search), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetLessonPlanApprovalStatusResult))]
        public Task<IActionResult> GetLessonPlanApprovalStatus(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/approval/status")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<LessonPlanApprovalStatusHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(LessonPlanEndpoint.GetLessonPlan))]
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
        [OpenApiParameter(nameof(GetLessonPlanRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetLessonPlanRequest.IdAcademicYear), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetLessonPlanRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetLessonPlanRequest.IdLevel), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetLessonPlanRequest.IdPeriod), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetLessonPlanRequest.IdSubject), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetLessonPlanRequest.Status), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetLessonPlanResult))]
        public Task<IActionResult> GetLessonPlan(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<LessonPlanHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(LessonPlanEndpoint.GetLessonPlanDocumentList))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetLessonPlanDocumentListRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetLessonPlanDocumentListRequest.IdLessonPlan), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetLessonPlanDocumentListRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetLessonPlanDocumentListRequest.IdLesson), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetLessonPlanDocumentListResult))]
        public Task<IActionResult> GetLessonPlanDocumentList(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/documents")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<LessonPlanDocumentListHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(LessonPlanEndpoint.AddLessonPlanDocument))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddLessonPlanDocumentRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> AddLessonPlanDocument(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "-document")] HttpRequest req,
            [Queue("notification-lp-lessonplan")] ICollector<string> collector,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<AddLessonPlanDocumentHandler>();
            return handler.Execute(req, cancellationToken, keyValues: nameof(collector).WithValue(collector));
        }

        [FunctionName(nameof(LessonPlanEndpoint.GetDetailLessonPlanDocument))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetDetailLessonPlanDocumentRequest.IdLessonPlanDocument), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetDetailLessonPlanDocumentResult))]
        public Task<IActionResult> GetDetailLessonPlanDocument(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-document-detail")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetDetailLessonPlanDocumentHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(LessonPlanEndpoint.GetDetailLessonPlanInformation))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetDetailLessonPlanInformationRequest.IdLessonPlan), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetDetailLessonPlanInformationResult))]
        public Task<IActionResult> GetDetailLessonPlanInformation(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-information-detail")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetDetailLessonPlanInformationHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(LessonPlanEndpoint.SetLessonPlanApprovalStatus))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(SetLessonPlanApprovalStatusRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> SetLessonPlanApprovalStatus(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/approval")] HttpRequest req,
            [Queue("notification-lp-lessonplan")] ICollector<string> collector,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<SetLessonPlanApprovalStatusHandler>();
            return handler.Execute(req, cancellationToken, keyValues: nameof(collector).WithValue(collector));
        }

        [FunctionName(nameof(LessonPlanEndpoint.GetSubjectLessonPlanApproval))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetSubjectLessonPlanApprovalRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSubjectLessonPlanApprovalRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSubjectLessonPlanApprovalRequest.IdLevel), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSubjectLessonPlanApprovalRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetSubjectLessonPlanApprovalResult[]))]
        public Task<IActionResult> GetSubjectLessonPlanApproval(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/approval/subject")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetSubjectLessonPlanApprovalHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(LessonPlanEndpoint.GetLessonPlanSummaryDetail))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetLessonPlanSummaryDetailRequest.IdWeekSettingDetail), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetLessonPlanSummaryDetailRequest.IdSubject), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetLessonPlanSummaryDetailRequest.IdAcademicYear), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetLessonPlanSummaryDetailRequest.IdUser), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetLessonPlanSummaryDetailRequest.PositionCode), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetLessonPlanSummaryDetailRequest.IdLessonTeacher), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetLessonPlanSummaryDetailResult))]
        public Task<IActionResult> GetLessonPlanSummaryDetail(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/summary/detail")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<LessonPlanSummaryDetailHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(LessonPlanEndpoint.GetLessonPlanApprovalSetting))]
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
        [OpenApiParameter(nameof(GetLessonPlanApprovalSettingRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetLessonPlanApprovalSettingRequest.IdLevel), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetLessonPlanApprovalSettingResult))]
        public Task<IActionResult> GetLessonPlanApprovalSetting(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-approval-setting")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<LessonPlanApprovalSettingHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(LessonPlanEndpoint.SetLessonPlanApprovalSetting))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(SetLessonPlanApprovalSettingRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> SetLessonPlanApprovalSetting(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "-approval-setting")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<SetLessonPlanApprovalSettingHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(LessonPlanEndpoint.GetLessonPlanApproval))]
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
        [OpenApiParameter(nameof(GetLessonPlanApprovalRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetLessonPlanApprovalRequest.IdAcademicYear), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetLessonPlanApprovalRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetLessonPlanApprovalRequest.IdLevel), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetLessonPlanApprovalRequest.IdPeriod), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetLessonPlanApprovalRequest.IdSubject), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetLessonPlanApprovalRequest.IdTeacher), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetLessonPlanApprovalResult))]
        public Task<IActionResult> GetLessonPlanApproval(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-approval")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<LessonPlanApprovalHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(LessonPlanEndpoint.GetLessonPlanSummary))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(CollectionRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetLessonPlanSummaryRequest.IdUser), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetLessonPlanSummaryRequest.IdAcademicYear), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetLessonPlanSummaryRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetLessonPlanSummaryRequest.IdLevel), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetLessonPlanSummaryRequest.IdPeriod), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetLessonPlanSummaryRequest.IdSubject), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetLessonPlanSummaryRequest.PositionCode), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetLessonPlanSummaryResult))]
        public Task<IActionResult> GetLessonPlanSummary(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-summary")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<LessonPlanListSummaryHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(LessonPlanEndpoint.GetDownloadLessonPlanSummary))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetDownloadLessonPlanSummaryRequest.IdAcademicYear), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDownloadLessonPlanSummaryRequest.IdUser), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDownloadLessonPlanSummaryRequest.PositionCode), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDownloadLessonPlanSummaryRequest.IdGrade), In = ParameterLocation.Query, Type = typeof(string[]))]
        [OpenApiParameter(nameof(GetDownloadLessonPlanSummaryRequest.IdSubject), In = ParameterLocation.Query, Type = typeof(string[]))]
        [OpenApiParameter(nameof(GetDownloadLessonPlanSummaryRequest.IsGrade), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetDownloadLessonPlanSummaryRequest.IsSubject), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetLessonPlanSummaryResult))]
        public Task<IActionResult> GetDownloadLessonPlanSummary(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-summary/download-lesson-plan-summary")] HttpRequest req,
           [Queue("notification-lp-lessonplan")] ICollector<string> collector,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetDownloadLessonPlanSummaryHandler>();
            return handler.Execute(req, cancellationToken, keyValues: nameof(collector).WithValue(collector));
        }

        [FunctionName(nameof(LessonPlanEndpoint.LessonPlanBlob))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(DeleteLessonPlanBlobRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> LessonPlanBlob(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "-summary/lesson-plan-blob")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<DeleteLessonPlanBlobHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(LessonPlanEndpoint.GetSubjectForLessonPlan))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetSubjectForLessonPlanRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSubjectForLessonPlanRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSubjectForLessonPlanRequest.IdLevel), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSubjectForLessonPlanRequest.IdGrade), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSubjectForLessonPlanRequest.Semester), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSubjectForLessonPlanRequest.Search), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetSubjectForLessonPlanResult))]
        public Task<IActionResult> GetSubjectForLessonPlan(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-subject-for-lesson-plan")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetSubjectForLessonPlanHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(LessonPlanEndpoint.LessonPlanLevelByPosition))]
        [OpenApiOperation(tags: _tag, Description = @"Get Data Level By Position")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(LessonPlanLevelByPositionRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(LessonPlanLevelByPositionRequest.SelectedPosition), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(LessonPlanLevelByPositionRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(IEnumerable<ItemValueVm>))]
        public Task<IActionResult> LessonPlanLevelByPosition(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/level-by-position")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<LessonPlanLevelByPositionHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(LessonPlanEndpoint.LessonPlanGradeByPosition))]
        [OpenApiOperation(tags: _tag, Description = @"Get Data Grade By Position")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(LessonPlanGradeByPositionRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(LessonPlanGradeByPositionRequest.SelectedPosition), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(LessonPlanGradeByPositionRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(LessonPlanGradeByPositionRequest.IdLevel), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(IEnumerable<ItemValueVm>))]
        public Task<IActionResult> LessonPlanGradeByPosition(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/grade-by-position")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<LessonPlanGradeByPositionHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(LessonPlanEndpoint.LessonPlanGradeByPositionV2))]
        [OpenApiOperation(tags: _tag, Description = @"Get Data Grade By Position")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(LessonPlanGradeByPositionV2Request.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(LessonPlanGradeByPositionV2Request.SelectedPosition), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(LessonPlanGradeByPositionV2Request.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(IEnumerable<ItemValueVm>))]
        public Task<IActionResult> LessonPlanGradeByPositionV2(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/grade-by-position-v2")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<LessonPlanGradeByPositionV2Handler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(GetLessonPlanApproverSetting))]
        [OpenApiOperation(tags: _tag, Summary = "Get Lesson Plan Approver Setting List")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetLessonPlanApproverSettingRequest.Ids), In = ParameterLocation.Query, Type = typeof(string[]))]
        [OpenApiParameter(nameof(GetLessonPlanApproverSettingRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetLessonPlanApproverSettingRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetLessonPlanApproverSettingRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetLessonPlanApproverSettingRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetLessonPlanApproverSettingRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetLessonPlanApproverSettingRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetLessonPlanApproverSettingRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetLessonPlanApproverSettingRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetLessonPlanApproverSettingResult))]
        public Task<IActionResult> GetLessonPlanApproverSetting(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/lesson-plan-approver-setting")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _lessonPlanApproverSettingHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AddLessonPlanApproverSetting))]
        [OpenApiOperation(tags: _tag, Summary = "Add Lesson Plan Approver Setting")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddLessonPlanApproverSettingRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> AddLessonPlanApproverSetting(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/lesson-plan-approver-setting")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _lessonPlanApproverSettingHandler.Execute(req, cancellationToken);
        }
    }
}

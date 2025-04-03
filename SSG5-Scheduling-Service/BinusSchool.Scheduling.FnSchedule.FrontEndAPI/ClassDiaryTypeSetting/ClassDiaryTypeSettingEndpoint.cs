using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Data.Model.Scheduling.FnSchedule.ClassDiaryTypeSetting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Scheduling.FnSchedule.ClassDiaryTypeSetting
{
    public class ClassDiaryTypeSettingEndpoint
    {
        private const string _route = "schedule/class-diary-type-setting";
        private const string _tag = "Class Diary Type Setting";

        [FunctionName(nameof(ClassDiaryTypeSettingEndpoint.GetListClassDiaryTypeSetting))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetClassDiaryTypeSettingRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetClassDiaryTypeSettingRequest.IdAcademicYear), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetClassDiaryTypeSettingRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetClassDiaryTypeSettingRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetClassDiaryTypeSettingRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetClassDiaryTypeSettingRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetClassDiaryTypeSettingRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetClassDiaryTypeSettingRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetClassDiaryTypeSettingRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetClassDiaryTypeSettingResult))]
        public Task<IActionResult> GetListClassDiaryTypeSetting(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<ClassDiaryTypeSettingHandler>();
            return handler.Execute(req, cancellationToken, false);
        }

        [FunctionName(nameof(ClassDiaryTypeSettingEndpoint.GetListClassDiaryLessonExcludeById))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetClassDiaryLessonExcludeRequest.IdClassDiaryTypeSetting), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetClassDiaryLessonExcludeRequest.Semester), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetClassDiaryLessonExcludeRequest.IdGrader), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetClassDiaryLessonExcludeRequest.IdSubject), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetClassDiaryLessonExcludeRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetClassDiaryLessonExcludeRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetClassDiaryLessonExcludeRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetClassDiaryLessonExcludeRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetClassDiaryLessonExcludeRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetClassDiaryLessonExcludeRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetClassDiaryLessonExcludeRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetClassDiaryLessonExcludeResult))]
        public Task<IActionResult> GetListClassDiaryLessonExcludeById(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/list-lesson-exclude")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetCLassDiaryLessonExcludeByIdHandler>();
            return handler.Execute(req, cancellationToken, false);
        }

        [FunctionName(nameof(ClassDiaryTypeSettingEndpoint.AddClassDiaryTypeSetting))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddClassDiaryTypeSettingRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> AddClassDiaryTypeSetting(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<ClassDiaryTypeSettingHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ClassDiaryTypeSettingEndpoint.UpdateClassDiaryTypeSetting))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateClassDiaryTypeSettingRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateClassDiaryTypeSetting(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<ClassDiaryTypeSettingHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ClassDiaryTypeSettingEndpoint.GetClassDiaryTypeSettingDetail))]
        [OpenApiOperation(tags: _tag, Summary = "Get Class Diary Type Setting Detail")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter("id", Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetClassDiaryTypeSettingDetailResult))]
        public Task<IActionResult> GetClassDiaryTypeSettingDetail(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/detail/{id}")] HttpRequest req,
            string id,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<ClassDiaryTypeSettingHandler>();
            return handler.Execute(req, cancellationToken, keyValues: "id".WithValue(id));
        }

        [FunctionName(nameof(ClassDiaryTypeSettingEndpoint.GetGradeLessonExclude))]
        [OpenApiOperation(tags: _tag, Summary = "Get Grade Lesson Exculde")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetGradeClassDiaryTypeSettingRequest.IdClassDiaryTypeSetting), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetGradeClassDiaryTypeSettingResult[]))]
        public Task<IActionResult> GetGradeLessonExclude(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/list-grade")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetGradeClassDiaryLessonExcludeHandler>();
            return handler.Execute(req, cancellationToken, false);
        }

        [FunctionName(nameof(ClassDiaryTypeSettingEndpoint.GetSubjectLessonExclude))]
        [OpenApiOperation(tags: _tag, Summary = "Get Subject Lesson Exculde")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetSubjectClassDiaryTypeSettingRequest.IdClassDiaryTypeSetting), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSubjectClassDiaryTypeSettingRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetSubjectClassDiaryTypeSettingResult[]))]
        public Task<IActionResult> GetSubjectLessonExclude(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/list-subject")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetSubjectClassDiaryLessonExcludeHandler>();
            return handler.Execute(req, cancellationToken, false);
        }

        [FunctionName(nameof(ClassDiaryTypeSettingEndpoint.DeleteClassDiaryTypeSetting))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Class Diary Type Setting")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(IEnumerable<string>), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> DeleteClassDiaryTypeSetting(
        [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route)] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<ClassDiaryTypeSettingHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ClassDiaryTypeSettingEndpoint.CopySettingClassDiaryTypeSetting))]
        [OpenApiOperation(tags: _tag, Summary = "Copy Class Diary Type Setting")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(CopySettingClassDiaryTypeSettingRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(CopySettingClassDiaryTypeSettingResult[]))]

        public Task<IActionResult> CopySettingClassDiaryTypeSetting(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/copy-setting")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<CopySettingClassDiaryTypeSettingHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ClassDiaryTypeSettingEndpoint.GetSameDataCopySettingClassDiaryTypeSetting))]
        [OpenApiOperation(tags: _tag, Summary = "Get Same Data Copy Class Diary Type Setting")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(CopySettingClassDiaryTypeSettingRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetSameDataClassDiaryTypeSettingResult))]

        public Task<IActionResult> GetSameDataCopySettingClassDiaryTypeSetting(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/get-same-data-copy-setting")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetSameDataClassDiaryTypeSettingHandler>();
            return handler.Execute(req, cancellationToken);
        }
    }
}

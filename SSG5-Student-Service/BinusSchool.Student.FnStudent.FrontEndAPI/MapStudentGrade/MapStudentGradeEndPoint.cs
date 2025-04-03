using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.OpenApi.Models;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.MapStudentGrade;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

namespace BinusSchool.Student.FnStudent.MapStudentGrade
{
    public class MapStudentGradeEndPoint
    {
        private const string _route = "student/map-student-grade";
        private const string _tag = "Mapping Student Grade";
        private readonly UploadExcelMapStudentGradeHandler _uploadExcelMapStudentGradeHandler;
        private readonly DownloadExcelMapStudentGradeHandler _downloadExcelMapStudentGradeHandler;

        public MapStudentGradeEndPoint(UploadExcelMapStudentGradeHandler uploadExcelMapStudentGradeHandler,
            DownloadExcelMapStudentGradeHandler downloadExcelMapStudentGradeHandler)
        {
            _uploadExcelMapStudentGradeHandler = uploadExcelMapStudentGradeHandler;
            _downloadExcelMapStudentGradeHandler = downloadExcelMapStudentGradeHandler;
        }

        [FunctionName(nameof(MapStudentGradeEndPoint.GetMapStudentGrades))]
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
        [OpenApiParameter(nameof(GetMapStudentGradeRequest.AcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetMapStudentGradeRequest.IdLevel), In = ParameterLocation.Query)]
        public Task<IActionResult> GetMapStudentGrades(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<MapStudentGradeHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MapStudentGradeEndPoint.CreateMapStudentGrades))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(CreateMapStudentGradeRequest))]
        [OpenApiResponseWithoutBody(System.Net.HttpStatusCode.OK)]
        public Task<IActionResult> CreateMapStudentGrades(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<MapStudentGradeHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MapStudentGradeEndPoint.CopyNextAYMapStudentGrade))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(CopyNextAYMapStudentGradeRequest))]
        [OpenApiResponseWithoutBody(System.Net.HttpStatusCode.OK)]
        public Task<IActionResult> CopyNextAYMapStudentGrade(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<MapStudentGradeHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MapStudentGradeEndPoint.GetDownloadTemplateMapStudentGrade))]
        [OpenApiOperation(tags: _tag, Summary = "Get Download Template Map Student by Grade")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiResponseWithoutBody(System.Net.HttpStatusCode.OK)]
        public Task<IActionResult> GetDownloadTemplateMapStudentGrade(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/download-excel")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _downloadExcelMapStudentGradeHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MapStudentGradeEndPoint.UploadExcelMapStudentGrade))]
        [OpenApiOperation(tags: _tag, Summary = "upload excel for mapping student grade")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(UploadExcelMapStudentGradeRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(UploadExcelMapStudentGradeRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        public Task<IActionResult> UploadExcelMapStudentGrade(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/upload-excel")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _uploadExcelMapStudentGradeHandler.Execute(req, cancellationToken);
        }
    }
}

using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Data.Model.Scheduling.FnSchedule.ClassOfficerSetting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Scheduling.FnSchedule.ClassOfficerSetting
{
    public class ClassOfficerSettingEndpoint
    {
        private const string _route = "schedule/class-officer-setting";
        private const string _tag = "Class Officer Setting";

        private readonly DownloadClassOfficerSettingHandler _downloadClassOfficerSettingHandler;

        public ClassOfficerSettingEndpoint(DownloadClassOfficerSettingHandler downloadClassOfficerSettingHandler)
        {
            _downloadClassOfficerSettingHandler = downloadClassOfficerSettingHandler;
        }

        [FunctionName(nameof(ClassOfficerSettingEndpoint.GetListClassOfficerSetting))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetClassOfficerSettingRequest.IdBinusian), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetClassOfficerSettingRequest.Position), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetClassOfficerSettingRequest.IdAcademicYear), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetClassOfficerSettingRequest.IdLevel), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetClassOfficerSettingRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetClassOfficerSettingRequest.IdHomeRoom), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetClassOfficerSettingRequest.Semester), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetClassOfficerSettingRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetClassOfficerSettingRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetClassOfficerSettingRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetClassOfficerSettingRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetClassOfficerSettingRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetClassOfficerSettingRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetClassOfficerSettingRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetClassOfficerSettingRequest))]
        public Task<IActionResult> GetListClassOfficerSetting(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<ClassOfficerSettingHandler>();
            return handler.Execute(req, cancellationToken, false);
        }

        [FunctionName(nameof(ClassOfficerSettingEndpoint.GetClassOfficerSettingDetail))]
        [OpenApiOperation(tags: _tag, Summary = "Get Class Officer Setting Detail")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter("id", Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetClassOfficerSettingDetailResult))]
        public Task<IActionResult> GetClassOfficerSettingDetail(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/{id}")] HttpRequest req,
            string id,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<ClassOfficerSettingHandler>();
            return handler.Execute(req, cancellationToken, keyValues: "id".WithValue(id));
        }

        [FunctionName(nameof(ClassOfficerSettingEndpoint.UpdateClassOfficerSetting))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateClassOfficerSettingRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateClassOfficerSetting(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<ClassOfficerSettingHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ClassOfficerSettingEndpoint.DownloadClassOfficerSetting))]
        [OpenApiOperation(tags: _tag, Summary = "Download Summary Class Officer Setting")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetClassOfficerSettingRequest.IdBinusian), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetClassOfficerSettingRequest.Position), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetClassOfficerSettingRequest.IdAcademicYear), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetClassOfficerSettingRequest.IdLevel), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetClassOfficerSettingRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetClassOfficerSettingRequest.IdHomeRoom), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetClassOfficerSettingRequest.Semester), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetClassOfficerSettingRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetClassOfficerSettingRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetClassOfficerSettingRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetClassOfficerSettingRequest))]
        public Task<IActionResult> DownloadClassOfficerSetting(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-download")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _downloadClassOfficerSettingHandler.Execute(req, cancellationToken);
        }
    }
}

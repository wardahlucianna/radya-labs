using BinusSchool.Data.Model.Teaching.FnAssignment.TeacherPositionInfo;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Teaching.FnAssignment.TeacherPositionInfo
{
    public class TeacherPositionInfoEndPoint
    {

        private const string _route = "teaching/TeacherPositionInfo";
        private const string _tag = "TeacherPositionInfo";

        private readonly GetDetailTeacherPositionHandler _getDetailTeacherPositionHandler;

        public TeacherPositionInfoEndPoint(GetDetailTeacherPositionHandler getDetailTeacherPositionHandler)
        {
            _getDetailTeacherPositionHandler = getDetailTeacherPositionHandler;
        }

        [FunctionName(nameof(TeacherPositionInfoEndPoint.GetTeacherPositionByUserID))]
        [OpenApiOperation(tags: _tag, Summary = "Get Teacher Position By UserID")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(GetTeacherPositionByUserIDRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetTeacherPositionByUserIDResult))]
        public Task<IActionResult> GetTeacherPositionByUserID(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/GetTeacherPositionByUserID")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetTeacherPositionByUserIdHandler>();
            return handler.Execute(req, cancellationToken, false);
        }

        [FunctionName(nameof(TeacherPositionInfoEndPoint.GetTeacherPositionsByUserID))]
        [OpenApiOperation(tags: _tag, Summary = "Get All Teacher Position By UserID")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(GetTeacherPositionByUserIDRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetTeacherPositionByUserIDResult))]
        public Task<IActionResult> GetTeacherPositionsByUserID(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/GetTeacherPositionByUserID/all")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetTeacherPositionByUserIdHandler>();
            return handler.Execute(req, cancellationToken, false);
        }

        [FunctionName(nameof(TeacherPositionInfoEndPoint.GetDetailTeacherPosition))]
        [OpenApiOperation(tags: _tag, Summary = "Get Detail Teacher Position By UserID")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetDetailTeacherPositionByUserIDRequest.UserId), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDetailTeacherPositionByUserIDRequest.IdSchoolAcademicYear), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetDetailTeacherPositionByUserIDResult))]
        public Task<IActionResult> GetDetailTeacherPosition(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/GetDetailTeacherPosition")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _getDetailTeacherPositionHandler.Execute(req, cancellationToken);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.ExtracurricularUnattendance;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Scheduling.FnExtracurricular.ExtracurricularUnattendance
{
    public class ExtracurricularUnattendanceEndPoint
    {
        private const string _route = "extracurricular-unattendance";
        private const string _tag = "Extracurricular Unattendance";

        [FunctionName(nameof(ExtracurricularUnattendanceEndPoint.GetExtracurricularUnattendance))]
        [OpenApiOperation(tags: _tag, Summary = "Get Extracurricular Unattendance List")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetExtracurricularUnattendanceRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetExtracurricularUnattendanceRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetExtracurricularUnattendanceRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetExtracurricularUnattendanceRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetExtracurricularUnattendanceRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetExtracurricularUnattendanceRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetExtracurricularUnattendanceRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetExtracurricularUnattendanceRequest.IdAcademicYear), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetExtracurricularUnattendanceRequest.Semester), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetExtracurricularUnattendanceResult))]
        public Task<IActionResult> GetExtracurricularUnattendance(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-extracurricular-unattendance")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetExtracurricularUnattendanceHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ExtracurricularUnattendanceEndPoint.AddExtracurricularUnattendance))]
        [OpenApiOperation(tags: _tag, Summary = "Add Extracurricular Unattendance")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddExtracurricularUnattendanceRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> AddExtracurricularUnattendance(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/add-extracurricular-unattendance")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<AddExtracurricularUnattendanceHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ExtracurricularUnattendanceEndPoint.DeleteExtracurricularUnattendance))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Extracurricular Unattendance")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(DeleteExtracurricularUnattendanceRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> DeleteExtracurricularUnattendance(
           [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route + "/delete-extracurricular-unattendance")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<DeleteExtracurricularUnattendanceHandler>();
            return handler.Execute(req, cancellationToken);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Teaching.FnAssignment.TeachingLoad;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Teaching.FnAssignment.TeachingLoad
{
    public class TeachingLoadEndpoint
    {
        private const string _route = "assignment/teaching-load";
        private const string _tag = "Teaching Load";

        private readonly TeachingLoadHandler _handler;

        public TeachingLoadEndpoint(TeachingLoadHandler handler)
        {
            _handler = handler;
        }

        [FunctionName(nameof(TeachingLoadEndpoint.GetTeacherLoads))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetTeacherLoadRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetTeacherLoadRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetTeacherLoadRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetTeacherLoadRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetTeacherLoadRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetTeacherLoadRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetTeacherLoadRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetTeacherLoadRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetTeacherLoadRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetTeacherLoadResult))]
        public Task<IActionResult> GetTeacherLoads(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _handler.Execute(req, cancellationToken);
        }
    }
}

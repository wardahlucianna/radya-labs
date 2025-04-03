using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Teaching.FnAssignment.TeacherAssignment.LHAndCA;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Teaching.FnAssignment.TeacherAssignment.LHAndCA
{
    public class TeacherAssignmentLHAndCAEndpoint
    {
        private const string _route = "assignment/teacher/lh-ca";
        private const string _tag = "Teacher Assignment LH And CA";

        [FunctionName(nameof(TeacherAssignmentLHAndCAEndpoint.GetTeacherAssignmentLHAndCA))]
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
        [OpenApiParameter(nameof(GetAssignLHAndCARequest.IdSchool), In = ParameterLocation.Query, Type = typeof(string[]), Required = true)]
        [OpenApiParameter(nameof(GetAssignLHAndCARequest.IdAcadyear), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAssignLHAndCARequest.IdLevel), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAssignLHAndCARequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetAssignLHAndCAResult))]
        public Task<IActionResult> GetTeacherAssignmentLHAndCA(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetAssignLHAndCAHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(TeacherAssignmentLHAndCAEndpoint.GetTeacherAssignmentLHAndCADetail))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetAssignLHAndCADetailRequest.Id), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAssignLHAndCADetailRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAssignLHAndCADetailRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetAssignLHAndCADetailResult))]
        public Task<IActionResult> GetTeacherAssignmentLHAndCADetail(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/detail")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetAssignLHAndCADetailHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(TeacherAssignmentLHAndCAEndpoint.AddTeacherAssignmentLHAndCA))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddAssignLHAndCARequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> AddTeacherAssignmentLHAndCA(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<AddAssignLHAndCAHandler>();
            return handler.Execute(req, cancellationToken);
        }
    }
}

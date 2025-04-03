using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Data.Model.Student.FnStudent.StudentPoint;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Student.FnStudent.StudentPoint
{
    public class StudentPointEndpoint
    {
        private const string _route = "student-point/student-point";
        private const string _tag = "Student";
        private readonly GetStudentPointSummaryHandler _studentPointSummary;
        private readonly GetDetailStudentMeritDemeritPointHandler _detailStudentMeritDemerit;

        public StudentPointEndpoint(GetStudentPointSummaryHandler studentPointSummary, GetDetailStudentMeritDemeritPointHandler detailStudentMeritDemerit)
        {
            _studentPointSummary = studentPointSummary;
            _detailStudentMeritDemerit = detailStudentMeritDemerit;
        }

        [FunctionName(nameof(StudentPointEndpoint.GetStudentPointSummary))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetStudentPointSummaryRequest.IdAcadyear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetStudentPointSummaryRequest.IdStudent), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetStudentPointSummaryResult))]
        public Task<IActionResult> GetStudentPointSummary(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "summary")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetStudentPointSummaryHandler>();
            return handler.Execute(req, cancellationToken, false);
        }

        [FunctionName(nameof(StudentPointEndpoint.GetDetailStudentMeritDemeritPoint))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetDetailStudentMeritDemeritPointRequest.IdAcadyear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDetailStudentMeritDemeritPointRequest.IdStudent), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDetailStudentMeritDemeritPointRequest.Semester), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetStudentPointSummaryResult))]
        public Task<IActionResult> GetDetailStudentMeritDemeritPoint(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "detail")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetDetailStudentMeritDemeritPointHandler>();
            return handler.Execute(req, cancellationToken, false);
        }
    }
}

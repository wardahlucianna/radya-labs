using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.StudentCertification;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Scheduling.FnSchedule.StudentCertification
{
    public class StudentCertificationEndpoint
    {
        private const string _route = "schedule/student-certification";
        private const string _tag = "Student Certification";

        [FunctionName(nameof(StudentCertificationEndpoint.GetListStudentCertification))]
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
        [OpenApiParameter(nameof(GetListStudentCertificationRequest.IdAcadyear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetListStudentCertificationRequest.Position), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetListStudentCertificationRequest.IdLevel), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListStudentCertificationRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListStudentCertificationRequest.IdPeriod), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetListStudentCertificationRequest.IdHomeroom), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListStudentCertificationRequest.ClassId), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetListStudentCertificationResult))]
        public Task<IActionResult> GetListStudentCertification(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetListStudentCertificationHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(StudentCertificationEndpoint.GetAcadYearByStudent))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetAcadYearByStudentRequest.IdStudent), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetAcadYearByStudentResult2))]
        public Task<IActionResult> GetAcadYearByStudent(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "schedule/get-acad-year-by-student")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetAcadYearByStudentHandler>();
            return handler.Execute(req, cancellationToken);
        }
    }
}

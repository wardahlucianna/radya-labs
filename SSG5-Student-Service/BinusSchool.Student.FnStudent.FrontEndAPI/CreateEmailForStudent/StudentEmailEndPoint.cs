using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Data.Model.Student.FnStudent.CreateEmailForStudent;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Student.FnStudent.CreateEmailForStudent
{
    public class StudentEmailEndPoint
    {
        private const string _route = "student/Email";
        private const string _tag = "StudentEmail";

        [FunctionName(nameof(StudentEmailEndPoint.GetStudentEmailRecommendation))]
        [OpenApiOperation(tags: _tag, Summary = "Get Student Email Recommendation")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(GetStudentEmailRecomendationRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetStudentEmailRecomendationResult))]
        public Task<IActionResult> GetStudentEmailRecommendation(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/GetStudentEmailRecommendation")] HttpRequest req,CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetStudentEmailRecomendationHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(StudentEmailEndPoint.CreateStudentEmail))]
        [OpenApiOperation(tags: _tag, Summary = "Create Student Email")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(CreateStudentEmailRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> CreateStudentEmail(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/CreateStudentEmail")] HttpRequest req, CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<CreateStudentEmailHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(StudentEmailEndPoint.CheckEmailAvailability))]
        [OpenApiOperation(tags: _tag, Summary = "Check Email Availability")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(CheckEmailAvailabilityRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetStudentEmailRecomendationResult))]
        public Task<IActionResult> CheckEmailAvailability(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/CheckEmailAvailability")] HttpRequest req, CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<CheckEmailAvailabilityHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(StudentEmailEndPoint.GetStudentEmailList))]
        [OpenApiOperation(tags: _tag, Summary = "Get StudentEmail List")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(GetStudentEmailListRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetStudentEmailListResult))]
        public Task<IActionResult> GetStudentEmailList(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/GetStudentEmailList")] HttpRequest req, CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetStudentEmailListHandler>();
            return handler.Execute(req, cancellationToken);
        }

    }
}

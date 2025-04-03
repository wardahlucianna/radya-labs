using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.School.FnSubject.Department;
using BinusSchool.Data.Model.Teaching.FnAssignment.TeacherAssignment.HODAndSH;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Teaching.FnAssignment.TeacherAssignment.HODAndSH
{
    public class TeacherAssignmentHODAndSHEndpoint
    {
        private const string _route = "assignment/teacher/hod-sh";
        private const string _tag = "Teacher Assignment HOD And SH";

        [FunctionName(nameof(TeacherAssignmentHODAndSHEndpoint.GetTeacherAssignmentHODAndSH))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetDepartmentRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDepartmentRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDepartmentRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetDepartmentRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetDepartmentRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDepartmentRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDepartmentRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetDepartmentRequest.IdSchool), In = ParameterLocation.Query, Type = typeof(string[]), Required = true)]
        [OpenApiParameter(nameof(GetDepartmentRequest.IdAcadyear), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDepartmentRequest.IdLevel), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetAssignHODAndSHResult))]
        public Task<IActionResult> GetTeacherAssignmentHODAndSH(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
          CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetAssignHODAndSHHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(TeacherAssignmentHODAndSHEndpoint.GetTeacherAssignmentHODAndSHDetail))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetAssignHODAndSHDetailRequest.IdSchool), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAssignHODAndSHDetailRequest.IdSchoolAcadYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAssignHODAndSHDetailRequest.IdSchoolDepartment), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetAssignHODAndSHDetailResult))]
        public Task<IActionResult> GetTeacherAssignmentHODAndSHDetail(
         [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/detail")] HttpRequest req,
         CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<AssignHODAndSHDetailHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(TeacherAssignmentHODAndSHEndpoint.AddTeacherAssignmentHODAndSH))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddAssignHODAndSHRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> AddTeacherAssignmentHODAndSH(
          [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route)] HttpRequest req,
          CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<AssignHODAndSHHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(TeacherAssignmentHODAndSHEndpoint.DeleteTeacherAssignmentHODAndSH))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(IEnumerable<string>), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> DeleteTeacherAssignmentHODAndSH(
        [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route)] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<AssignHODAndSHHandler>();
            return handler.Execute(req, cancellationToken);
        }
    }
}

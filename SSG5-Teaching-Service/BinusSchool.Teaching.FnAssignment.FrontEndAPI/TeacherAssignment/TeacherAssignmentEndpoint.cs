using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Teaching.FnAssignment.NonTeachingLoad;
using BinusSchool.Data.Model.Teaching.FnAssignment.TeacherAssignment;
using BinusSchool.Teaching.FnAssignment.NonTeachingLoad;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Teaching.FnAssignment.TeacherAssignment
{
    public class TeacherAssignmentEndpoint
    {
        private const string _route = "assignment/teacher";
        private const string _tag = "Teacher Assignment";
        private readonly TeacherAssignmentHandler _handler;
        private readonly TeacherAssignmentDetailHandler _detailHandler;
       
        public TeacherAssignmentEndpoint(TeacherAssignmentHandler handler, TeacherAssignmentDetailHandler detailHandler)
        {
            _handler = handler;
            _detailHandler = detailHandler;
        }

        [FunctionName(nameof(TeacherAssignmentEndpoint.GetTeacherAssignments))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(TeacherAssignmentGetListRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(TeacherAssignmentGetListRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(TeacherAssignmentGetListRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(TeacherAssignmentGetListRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(TeacherAssignmentGetListRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(TeacherAssignmentGetListRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(TeacherAssignmentGetListRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(TeacherAssignmentGetListRequest.IdSchool), In = ParameterLocation.Query, Type = typeof(string[]), Required = true)]
        [OpenApiParameter(nameof(TeacherAssignmentGetListRequest.IdAcadyear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(TeacherAssignmentGetListRequest.IdLevel), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(TeacherAssignmentGetListRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(TeacherAssignmentGetListRequest.IdClass), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(TeacherAssignmentGetListRequest.Status), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(TeacherAssignmentGetListResult))]
        public Task<IActionResult> GetTeacherAssignments(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
          CancellationToken cancellationToken)
        {
            return _handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(TeacherAssignmentEndpoint.GetTeacherAssignmentDetail))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(TeacherAssignmentDetailRequest.IdSchoolUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(TeacherAssignmentDetailRequest.IdSchoolAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(TeacherAssignmentGetDetailResult))]
        public Task<IActionResult> GetTeacherAssignmentDetail(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/detail")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            return _detailHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(TeacherAssignmentEndpoint.AddTeacherAssignment))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddTeacherAssignmentRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> AddTeacherAssignment(
          [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route)] HttpRequest req,
          CancellationToken cancellationToken)
        {
            return _handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(TeacherAssignmentEndpoint.DeleteNonAcademic))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(IEnumerable<string>), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> DeleteNonAcademic(
         [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route + "/delete-non-academic")] HttpRequest req,
         CancellationToken cancellationToken)
        {
            return _handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(TeacherAssignmentEndpoint.DeleteAcademic))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(IEnumerable<string>), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> DeleteAcademic(
         [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route + "/delete-academic")] HttpRequest req,
         CancellationToken cancellationToken)
        {
            return _handler.Execute(req, cancellationToken);
        }

        #region copy Next Academic Year
        [FunctionName(nameof(TeacherAssignmentEndpoint.TeacherAssignmentCopy))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(TeacherAssignmentCopyRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> TeacherAssignmentCopy(
         [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/copy")] HttpRequest req,
         CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<TeacherAssignmentCopyHandler>();
            return handler.Execute(req, cancellationToken, false);
        }

        [FunctionName(nameof(TeacherAssignmentEndpoint.GetTeacherAssignmentCopy))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetTeacherAssignmentCopyRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetTeacherAssignmentCopyRequest.IdAcademicYearFrom), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetTeacherAssignmentCopyRequest.IdAcademicYearTo), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetTeacherAssignmentCopyRequest.Category), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetTeacherAssignmentCopyResult))]
        public Task<IActionResult> GetTeacherAssignmentCopy(
         [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/copy")] HttpRequest req,
         CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetTeacherAssignmentCopyHandler>();
            return handler.Execute(req, cancellationToken, false);
        }
        #endregion

    }
}

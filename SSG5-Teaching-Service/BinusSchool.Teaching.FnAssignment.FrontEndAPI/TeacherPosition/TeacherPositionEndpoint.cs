using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model;
using BinusSchool.Data.Model.Teaching.FnAssignment.TeacherAssignment;
using BinusSchool.Data.Model.Teaching.FnAssignment.TeacherPosition;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Teaching.FnAssignment.TeacherPosition
{
    public class TeacherPositionEndpoint
    {
        private const string _route = "assignment/teacher/position";
        private const string _tag = "Teacher Position";

        private readonly TeacherPositionHandler _handler;
        private readonly GetPositionCAForAcTimeTableHanlder _getCaPosition;
        private readonly TeacherPositionHasNotNonTeachingLoadHandler _positionHasNotNonTeachingLoadHandler;
        private readonly TeacherPositionByRoleHandler _teacherPositionByRole;

        public TeacherPositionEndpoint(TeacherPositionHandler handler,
            GetPositionCAForAcTimeTableHanlder getCaPosition, TeacherPositionHasNotNonTeachingLoadHandler positionHasNotNonTeachingLoadHandler,
            TeacherPositionByRoleHandler teacherPositionByRole)
        {
            _handler = handler;
            _getCaPosition = getCaPosition;
            _positionHasNotNonTeachingLoadHandler = positionHasNotNonTeachingLoadHandler;
            _teacherPositionByRole = teacherPositionByRole;
        }

        [FunctionName(nameof(TeacherPositionEndpoint.GetTeacherPositions))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetTeacherPositionRequest.Ids), In = ParameterLocation.Query, Type = typeof(string[]))]
        [OpenApiParameter(nameof(GetTeacherPositionRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetTeacherPositionRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetTeacherPositionRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetTeacherPositionRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetTeacherPositionRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetTeacherPositionRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetTeacherPositionRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetTeacherPositionRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetTeacherPositionRequest.PositionCode), In = ParameterLocation.Query, Type = typeof(string[]))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetTeacherPositionResult))]
        public Task<IActionResult> GetTeacherPositions(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(TeacherPositionEndpoint.GetTeacherPositionDetail))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter("id", Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetTeacherPositionDetailResult))]
        public Task<IActionResult> GetTeacherPositionDetail(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/{id}")] HttpRequest req,
           string id,
           CancellationToken cancellationToken)
        {
            return _handler.Execute(req, cancellationToken, keyValues: "id".WithValue(id));
        }

        [FunctionName(nameof(TeacherPositionEndpoint.GetCaPositionForAsc))]
        [OpenApiOperation(tags: _tag, Summary = "Get Position Ca For Asc Timetable")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetCAForAscTimetableRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetTeacherPositionDetailResult))]
        public Task<IActionResult> GetCaPositionForAsc(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-ca")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _getCaPosition.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(TeacherPositionEndpoint.AddTeacherPosition))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddTeacherPositionRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> AddTeacherPosition(
           [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route)] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(TeacherPositionEndpoint.UpdateTeacherPosition))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateTeacherPositionRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateTeacherPosition(
           [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route)] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(TeacherPositionEndpoint.DeleteTeacherPosition))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(IEnumerable<string>), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> DeleteTeacherPosition(
          [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route)] HttpRequest req,
          CancellationToken cancellationToken)
        {
            return _handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(TeacherPositionEndpoint.GetTeacherPositionsHasNotNonTeachingLoad))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetTeacherPositionHasNotNonTeachingLoadRequest.Ids), In = ParameterLocation.Query, Type = typeof(string[]))]
        [OpenApiParameter(nameof(GetTeacherPositionHasNotNonTeachingLoadRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetTeacherPositionHasNotNonTeachingLoadRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetTeacherPositionHasNotNonTeachingLoadRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetTeacherPositionHasNotNonTeachingLoadRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetTeacherPositionHasNotNonTeachingLoadRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetTeacherPositionHasNotNonTeachingLoadRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetTeacherPositionHasNotNonTeachingLoadRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetTeacherPositionHasNotNonTeachingLoadRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetTeacherPositionHasNotNonTeachingLoadRequest.IdAcadyear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetTeacherPositionHasNotNonTeachingLoadRequest.PositionCode), In = ParameterLocation.Query, Type = typeof(string))]
        [OpenApiParameter(nameof(GetTeacherPositionHasNotNonTeachingLoadRequest.Category), In = ParameterLocation.Query, Required = true)]

        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetTeacherPositionHasNotNonTeachingLoadRequest))]
        public Task<IActionResult> GetTeacherPositionsHasNotNonTeachingLoad(
         [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-has-not-non-teaching-load")] HttpRequest req,
         CancellationToken cancellationToken)
        {
            return _positionHasNotNonTeachingLoadHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(TeacherPositionEndpoint.GetTeacherPositionByRole))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetTeacherPositionByRoleRequest.IdRole), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<ItemValueVm>))]
        public Task<IActionResult> GetTeacherPositionByRole(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/role")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _teacherPositionByRole.Execute(req, cancellationToken);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Employee.FnStaff;
using BinusSchool.Data.Model.Employee.FnStaff.Teacher;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Employee.FnStaff.Teacher
{
    public class TeacherEndpoint
    {
        private const string _route = "employee/teacher";
        private const string _tag = "Teacher Information";

        private readonly TeacherHandler _teacherHandler;
        private readonly UpdateExpatriateFormalitiesHandler _updateExpatriateFormalitiesHandler;
        private readonly GetTeacherForAscTimetableHandler _getTeacherForAscTimetableHandler;

        public TeacherEndpoint(TeacherHandler teacherHandler
            , UpdateExpatriateFormalitiesHandler updateExpatriateFormalitiesHandler
            , GetTeacherForAscTimetableHandler getTeacherForAscTimetableHandler)
        {
            _teacherHandler = teacherHandler;
            _updateExpatriateFormalitiesHandler = updateExpatriateFormalitiesHandler;
            _getTeacherForAscTimetableHandler = getTeacherForAscTimetableHandler;
        }

        [FunctionName(nameof(TeacherEndpoint.GetteacherForASC))]
        [OpenApiOperation(tags: _tag, Summary = "get teacher for Upload XML asc timetable ")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(CheckTeacherForAscTimetableRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(CheckTeacherForAscTimetableResult))]
        public Task<IActionResult> GetteacherForASC(
          [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/get-teacher-for-asc")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            return _getTeacherForAscTimetableHandler.Execute(req, cancellationToken);
        }

        /*[FunctionName(nameof(TeacherEndpoint.GetTeachers))]
        [OpenApiOperation(tags: _tag, Summary = "Get Teacher List")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(CollectionRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetTeacherRequest.IdSchool), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetTeacherResult))]
        public Task<IActionResult> GetTeachers(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "_information")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _teacherHandler.Execute(req, cancellationToken);
        }*/

        [FunctionName(nameof(TeacherEndpoint.GetTeacherDetail))]
        [OpenApiOperation(tags: _tag, Summary = "Get Teacher Detail")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter("id", Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetTeacherDetailResult))]
        public Task<IActionResult> GetTeacherDetail(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-information/{id}")] HttpRequest req,
           string id,
           CancellationToken cancellationToken)
        {
            return _teacherHandler.Execute(req, cancellationToken, true, "id".WithValue(id));
        }

        [FunctionName(nameof(TeacherEndpoint.UpdateExpatriateFormalities))]
        [OpenApiOperation(tags: _tag, Summary = "Update Expatriate Formalities")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateExpatriateFormalitiesRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateExpatriateFormalities(
           [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route + "-update-expatriate-formalities")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _updateExpatriateFormalitiesHandler.Execute(req, cancellationToken);
        }
    }
}

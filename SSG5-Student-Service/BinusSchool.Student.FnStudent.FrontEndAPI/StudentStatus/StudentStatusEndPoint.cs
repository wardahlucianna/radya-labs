using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Data.Model.Student.FnStudent.StudentStatus;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Student.FnStudent.StudentStatus
{
    public class StudentStatusEndPoint
    {
        private const string _route = "student/student-status";
        private const string _tag = "Student Status";

        private readonly LtStudentStatusHandler _ltStudentStatusHandler;
        private readonly GetStudentStatusViewConfigurationHandler _getStudentStatusViewConfigurationHandler;
        private readonly GetStudentStatusListByAYHandler _getStudentStatusListByAYHandler;
        private readonly UpdateStudentStatusHandler _updateStudentStatusHandler;
        private readonly CreateStudentStatusRecordHandler _createStudentStatusRecordHandler;
        private readonly GetStudentStatusHistoryHandler _getStudentStatusHistoryHandler;
        private readonly GetUnmappedStudentStatusByAYHandler _getUnmappedStudentStatusByAYHandler;
        private readonly GenerateStudentStatusMappingActiveAYHandler _generateStudentStatusMappingActiveAYHandler;

        public StudentStatusEndPoint(
            LtStudentStatusHandler ltStudentStatusHandler,
            GetStudentStatusViewConfigurationHandler getStudentStatusViewConfigurationHandler,
            GetStudentStatusListByAYHandler getStudentStatusListByAYHandler,
            UpdateStudentStatusHandler updateStudentStatusHandler,
            CreateStudentStatusRecordHandler createStudentStatusRecordHandler,
            GetStudentStatusHistoryHandler getStudentStatusHistoryHandler,
            GetUnmappedStudentStatusByAYHandler getUnmappedStudentStatusByAYHandler,
            GenerateStudentStatusMappingActiveAYHandler generateStudentStatusMappingActiveAYHandler)
        {
            _ltStudentStatusHandler = ltStudentStatusHandler;
            _getStudentStatusViewConfigurationHandler = getStudentStatusViewConfigurationHandler;
            _getStudentStatusListByAYHandler = getStudentStatusListByAYHandler;
            _updateStudentStatusHandler = updateStudentStatusHandler;
            _createStudentStatusRecordHandler = createStudentStatusRecordHandler;
            _getStudentStatusHistoryHandler = getStudentStatusHistoryHandler;
            _getUnmappedStudentStatusByAYHandler = getUnmappedStudentStatusByAYHandler;
            _generateStudentStatusMappingActiveAYHandler = generateStudentStatusMappingActiveAYHandler;
        }

        [FunctionName(nameof(StudentStatusEndPoint.GetLtStudentStatus))]
        [OpenApiOperation(tags: _tag, Summary = "Get Lt Student Status")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetLtStudentStatusRequest.Ids), In = ParameterLocation.Query, Type = typeof(string[]))]
        [OpenApiParameter(nameof(GetLtStudentStatusRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetLtStudentStatusRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetLtStudentStatusRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetLtStudentStatusRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetLtStudentStatusRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetLtStudentStatusRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetLtStudentStatusRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetLtStudentStatusResult))]
        public Task<IActionResult> GetLtStudentStatus(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/lt-student-status")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _ltStudentStatusHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(StudentStatusEndPoint.GetStudentStatusViewConfiguration))]
        [OpenApiOperation(tags: _tag, Summary = "Get Student Status View Configuration")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetStudentStatusViewConfigurationRequest.IdSchool), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetStudentStatusViewConfigurationRequest.IdAcademicYear), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetStudentStatusViewConfigurationResult))]
        public Task<IActionResult> GetStudentStatusViewConfiguration(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/student-status-view-configuration")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _getStudentStatusViewConfigurationHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(StudentStatusEndPoint.GetStudentStatusListByAY))]
        [OpenApiOperation(tags: _tag, Summary = "Get Student Status List By Academic Year")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetStudentStatusListByAYRequest.Ids), In = ParameterLocation.Query, Type = typeof(string[]))]
        [OpenApiParameter(nameof(GetStudentStatusListByAYRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetStudentStatusListByAYRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetStudentStatusListByAYRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetStudentStatusListByAYRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetStudentStatusListByAYRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetStudentStatusListByAYRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetStudentStatusListByAYRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetStudentStatusListByAYRequest.IdSchool), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetStudentStatusListByAYRequest.IdAcademicYear), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetStudentStatusListByAYRequest.Semester), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetStudentStatusListByAYRequest.IdLevel), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetStudentStatusListByAYRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetStudentStatusListByAYRequest.IdHomeroom), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetStudentStatusListByAYRequest.IdStudentStatus), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetStudentStatusListByAYRequest.SearchStudentKeyword), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetStudentStatusListByAYResult))]
        public Task<IActionResult> GetStudentStatusListByAY(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-student-status-list-by-ay")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _getStudentStatusListByAYHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(StudentStatusEndPoint.UpdateStudentStatus))]
        [OpenApiOperation(tags: _tag, Summary = "Update Student Status")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateStudentStatusRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateStudentStatus(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/update")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _updateStudentStatusHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(StudentStatusEndPoint.CreateStudentStatusRecord))]
        [OpenApiOperation(tags: _tag, Summary = "Create Student Status Record")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(CreateStudentStatusRecordRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> CreateStudentStatusRecord(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/create-record")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _createStudentStatusRecordHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(StudentStatusEndPoint.GetStudentStatusHistory))]
        [OpenApiOperation(tags: _tag, Summary = "Get Student Status History")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetStudentStatusHistoryRequest.IdStudent), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetStudentStatusHistoryResult))]
        public Task<IActionResult> GetStudentStatusHistory(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-history")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _getStudentStatusHistoryHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(StudentStatusEndPoint.GetUnmappedStudentStatusByAY))]
        [OpenApiOperation(tags: _tag, Summary = "Get Unmapped Student Status by Academic Year")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetUnmappedStudentStatusByAYRequest.Ids), In = ParameterLocation.Query, Type = typeof(string[]))]
        [OpenApiParameter(nameof(GetUnmappedStudentStatusByAYRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetUnmappedStudentStatusByAYRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetUnmappedStudentStatusByAYRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetUnmappedStudentStatusByAYRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetUnmappedStudentStatusByAYRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetUnmappedStudentStatusByAYRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetUnmappedStudentStatusByAYRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetUnmappedStudentStatusByAYRequest.IdAcademicYear), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetUnmappedStudentStatusByAYResult))]
        public Task<IActionResult> GetUnmappedStudentStatusByAY(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-unmapped-student-status-by-ay")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _getUnmappedStudentStatusByAYHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(StudentStatusEndPoint.GenerateStudentStatusMappingActiveAY))]
        [OpenApiOperation(tags: _tag, Summary = "Generate Student Status Mapping By Active AY")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(GenerateStudentStatusMappingActiveAYRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> GenerateStudentStatusMappingActiveAY(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/generate-mapping-active-ay")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _generateStudentStatusMappingActiveAYHandler.Execute(req, cancellationToken);
        }
    }
}

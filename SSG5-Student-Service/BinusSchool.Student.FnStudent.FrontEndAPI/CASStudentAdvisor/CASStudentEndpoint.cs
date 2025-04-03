using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Data.Model.Student.FnStudent.CASStudentAdvisor;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Student.FnStudent.CASStudentAdvisor
{
    public class CASStudentEndpoint
    {
        private const string _route = "casstudentadvisor";
        private const string _tag = "CAS Student Advisor";

        private readonly GetFilterCASStudentAdvisorHandler _getFilterCASStudentAdvisorHandler;
        private readonly GetListCASStudentAdvisorHandler _getListCASStudentAdvisorHandler;
        private readonly SaveCasStudentMappingAdvisorHandler _saveCasStudentMappingAdvisorHandler;
        private readonly GetListCASAdvisorHandler _getListCASAdvisorHandler;
        private readonly DeleteCASAdvisorHandler _deleteCASAdvisorHandler;
        private readonly GetListTeacherForCASHandler _getListTeacherForCASHandler;
        private readonly AddCASAdvisorHandler _addCASAdvisorHandler;

        public CASStudentEndpoint(
                GetFilterCASStudentAdvisorHandler getFilterCASStudentAdvisorHandler,
                GetListCASStudentAdvisorHandler getListCASStudentAdvisorHandler,
                SaveCasStudentMappingAdvisorHandler saveCasStudentMappingAdvisorHandler,
                GetListCASAdvisorHandler getListCASAdvisorHandler,
                DeleteCASAdvisorHandler deleteCASAdvisorHandler,
                GetListTeacherForCASHandler getListTeacherForCASHandler,
                AddCASAdvisorHandler addCASAdvisorHandler
            )
        {
            _getFilterCASStudentAdvisorHandler = getFilterCASStudentAdvisorHandler;
            _getListCASStudentAdvisorHandler = getListCASStudentAdvisorHandler;
            _saveCasStudentMappingAdvisorHandler = saveCasStudentMappingAdvisorHandler;
            _getListCASAdvisorHandler = getListCASAdvisorHandler;
            _deleteCASAdvisorHandler = deleteCASAdvisorHandler;
            _getListTeacherForCASHandler = getListTeacherForCASHandler;
            _addCASAdvisorHandler = addCASAdvisorHandler;
        }

        [FunctionName(nameof(CASStudentEndpoint.GetListFilterAdvisorByGradeAcademicYear))]
        [OpenApiOperation(tags: _tag, Summary = "Get List Filter advisor By Academic Year")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetFilterCASStudentAdvisorRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetFilterCASStudentAdvisorRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetFilterCASStudentAdvisorResult>))]
        public Task<IActionResult> GetListFilterAdvisorByGradeAcademicYear(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-list-filter-advisor")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _getFilterCASStudentAdvisorHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(CASStudentEndpoint.GetCASStudentAdvisor))]
        [OpenApiOperation(tags: _tag, Summary = "Get List Student Advisor")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetListCASStudentAdvisorRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetListCASStudentAdvisorRequest.IdGrade), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetListCASStudentAdvisorRequest.IdCASAdvisor), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListCASStudentAdvisorRequest.HomeroomCode), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetListCASStudentAdvisorResult>))]
        public Task<IActionResult> GetCASStudentAdvisor(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/list-student-advisor")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _getListCASStudentAdvisorHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(CASStudentEndpoint.SaveCasStudentMapping))]
        [OpenApiOperation(tags: _tag, Summary = "Save CAS Student Mapping")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(SaveCasStudentMappingAdvisorRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> SaveCasStudentMapping(
        [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route + "/save-cas-student-mapping")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _saveCasStudentMappingAdvisorHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(CASStudentEndpoint.GetListCASAdvisor))]
        [OpenApiOperation(tags: _tag, Summary = "Get List Advisor")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetListCASAdvisorRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetListCASAdvisorRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListCASAdvisorRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListCASAdvisorRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetListCASAdvisorRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetListCASAdvisorRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListCASAdvisorRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListCASAdvisorRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetListCASAdvisorResult>))]
        public Task<IActionResult> GetListCASAdvisor(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-list-advisor")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _getListCASAdvisorHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(CASStudentEndpoint.DeleteCASAdvisor))]
        [OpenApiOperation(tags: _tag, Summary = "Delete CAS Advisor")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(DeleteCASAdvisorRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> DeleteCASAdvisor(
        [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route + "/delete-cas-advisor")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _deleteCASAdvisorHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(CASStudentEndpoint.GetListTeacherForCAS))]
        [OpenApiOperation(tags: _tag, Summary = "Get List Teacher to be Advisor")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetListTeacherForCASRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetListTeacherForCASRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetListTeacherForCASResult>))]
        public Task<IActionResult> GetListTeacherForCAS(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-list-teacher-for-cas")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _getListTeacherForCASHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(CASStudentEndpoint.AddCASAdvisor))]
        [OpenApiOperation(tags: _tag, Summary = "Add CAS Advisor")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddCASAdvisorRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> AddCASAdvisor(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/add-cas-advisor")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _addCASAdvisorHandler.Execute(req, cancellationToken);
        }
    }
}

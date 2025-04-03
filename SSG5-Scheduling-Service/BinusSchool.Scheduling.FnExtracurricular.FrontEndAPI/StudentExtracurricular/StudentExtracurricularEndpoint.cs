using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.StudentExtracurricular;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Scheduling.FnExtracurricular.StudentExtracurricular
{
    public class StudentExtracurricularEndpoint
    {
        private const string _route = "student-extracurricular";
        private const string _tag = "Student Extracurricular";

        private readonly GetStudentExtracurricularHandler _getStudentExtracurricularHandler;
        private readonly GetDetailStudentExtracurricularHandler _getDetailStudentExtracurricularHandler;
        private readonly UpdateStudentExtracurricularPriorityHandler _updateStudentExtracurricularPriorityHandler;
        private readonly ExportExcelStudentExtracurricularHandler _exportExcelStudentExtracurricularHandler;

        public StudentExtracurricularEndpoint(
            GetStudentExtracurricularHandler getStudentExtracurricularHandler,
            GetDetailStudentExtracurricularHandler getDetailStudentExtracurricularHandler,
            UpdateStudentExtracurricularPriorityHandler updateStudentExtracurricularPriorityHandler,
            ExportExcelStudentExtracurricularHandler exportExcelStudentExtracurricularHandler)
        {
            _getStudentExtracurricularHandler = getStudentExtracurricularHandler;
            _getDetailStudentExtracurricularHandler = getDetailStudentExtracurricularHandler;
            _updateStudentExtracurricularPriorityHandler = updateStudentExtracurricularPriorityHandler;
            _exportExcelStudentExtracurricularHandler = exportExcelStudentExtracurricularHandler;
        }

        [FunctionName(nameof(StudentExtracurricularEndpoint.GetStudentExtracurriculara))]
        [OpenApiOperation(tags: _tag, Summary = "Get Student Extracurricular", Description = @"")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(GetStudentExtracurricularRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetStudentExtracurricularResult))]
        public Task<IActionResult> GetStudentExtracurriculara([HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/get-student-extracurricular")] HttpRequest req,
       CancellationToken cancellationToken)
        {
            return _getStudentExtracurricularHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(StudentExtracurricularEndpoint.GetDetailStudentExtracurriculara))]
        [OpenApiOperation(tags: _tag, Summary = "Get Detail Student Extracurricular By Student")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetDetailStudentExtracurricularRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDetailStudentExtracurricularRequest.IdLevel), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDetailStudentExtracurricularRequest.IdGrade), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDetailStudentExtracurricularRequest.Semester), In = ParameterLocation.Query, Required = true, Type = typeof(int))]
        [OpenApiParameter(nameof(GetDetailStudentExtracurricularRequest.IdStudent), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetDetailStudentExtracurricularResult))]
        public Task<IActionResult> GetDetailStudentExtracurriculara([HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-detail-student-extracurricular")] HttpRequest req,
       CancellationToken cancellationToken)
        {
            return _getDetailStudentExtracurricularHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(StudentExtracurricularEndpoint.UpdateStudentExtracurricularPriority))]
        [OpenApiOperation(tags: _tag, Summary = "Update Student Extracurricular Priority")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateStudentExtracurricularPriorityRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateStudentExtracurricularPriority([HttpTrigger(AuthorizationLevel.Function, "put", Route = _route + "/update-student-extracurricular-priority")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _updateStudentExtracurricularPriorityHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(StudentExtracurricularEndpoint.ExportExcelStudentExtracurricular))]
        [OpenApiOperation(tags: _tag, Summary = "Export Student Extracurricular to Excel")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(ExportExcelStudentExtracurricularRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> ExportExcelStudentExtracurricular([HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/student-extracurricular-excel")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _exportExcelStudentExtracurricularHandler.Execute(req, cancellationToken, false);
        }
    }
}

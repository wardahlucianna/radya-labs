using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Shared.Kernel.DataModel.Student.FnStudent.SubjectSelection.CurriculumSettings;
using BinusSchool.Student.BLL.FnStudent.SubjectSelection.CurriculumSettings.SubjectSelectionCurriculum;
using BinusSchool.Student.BLL.FnStudent.SubjectSelection.CurriculumSettings.SubjectSelectionCurriculumMapping;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Student.FnStudent.FrontEndAPI.SubjectSelection.CuriculumSettings
{
    public class CurriculumSettingsEndpoint
    {
        private const string _route = "student/curriculum-settings";
        private const string _tag = "Subject Selection (Curriculum Settings)";

        private readonly GetSubjectSelectionCurriculumHandler _getSubjectSelectionCurriculumHandler;
        private readonly SaveSubjectSelectionCurriculumHandler _saveSubjectSelectionCurriculumHandler;
        private readonly DeleteSubjectSelectionCurriculumHandler _deleteSubjectSelectionCurriculumHandler;

        private readonly GetListSubjectSelectionCurriculumMappingHandler _getListSubjectSelectionCurriculumMappingHandler;
        private readonly GetDetailSubjectSelectionCurriculumMappingHandler _getDetailSubjectSelectionCurriculumMappingHandler;
        private readonly SaveSubjectSelectionCurriculumMappingHandler _saveSubjectSelectionCurriculumMappingHandler;
        private readonly DeleteSubjectSelectionCurriculumMappingHandler _deleteSubjectSelectionCurriculumMappingHandler;
        private readonly CopySubjectSelectionCurriculumMappingHandler _copySubjectSelectionCurriculumMappingHandler;

        public CurriculumSettingsEndpoint(
            GetSubjectSelectionCurriculumHandler getSubjectSelectionCurriculumHandler,
            SaveSubjectSelectionCurriculumHandler saveSubjectSelectionCurriculumHandler,
            DeleteSubjectSelectionCurriculumHandler deleteSubjectSelectionCurriculumHandler,
            GetListSubjectSelectionCurriculumMappingHandler getListSubjectSelectionCurriculumMappingHandler,
            GetDetailSubjectSelectionCurriculumMappingHandler getDetailSubjectSelectionCurriculumMappingHandler,
            SaveSubjectSelectionCurriculumMappingHandler saveSubjectSelectionCurriculumMappingHandler,
            DeleteSubjectSelectionCurriculumMappingHandler deleteSubjectSelectionCurriculumMappingHandler,
            CopySubjectSelectionCurriculumMappingHandler copySubjectSelectionCurriculumMappingHandler)
        {
            _getSubjectSelectionCurriculumHandler = getSubjectSelectionCurriculumHandler;
            _saveSubjectSelectionCurriculumHandler = saveSubjectSelectionCurriculumHandler;
            _deleteSubjectSelectionCurriculumHandler = deleteSubjectSelectionCurriculumHandler;

            _getListSubjectSelectionCurriculumMappingHandler = getListSubjectSelectionCurriculumMappingHandler;
            _getDetailSubjectSelectionCurriculumMappingHandler = getDetailSubjectSelectionCurriculumMappingHandler;
            _saveSubjectSelectionCurriculumMappingHandler = saveSubjectSelectionCurriculumMappingHandler;
            _deleteSubjectSelectionCurriculumMappingHandler = deleteSubjectSelectionCurriculumMappingHandler;
            _copySubjectSelectionCurriculumMappingHandler = copySubjectSelectionCurriculumMappingHandler;
        }

        [FunctionName(nameof(GetSubjectSelectionCurriculum))]
        [OpenApiOperation(tags: _tag, Summary = "Get Subject Selection Curriculum")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetSubjectSelectionCurriculumRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetSubjectSelectionCurriculumResult>))]
        public Task<IActionResult> GetSubjectSelectionCurriculum(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-subject-selection-curriculum")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _getSubjectSelectionCurriculumHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(SaveSubjectSelectionCurriculum))]
        [OpenApiOperation(tags: _tag, Summary = "Save Subject Selection Curriculum")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(SaveSubjectSelectionCurriculumRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> SaveSubjectSelectionCurriculum(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/save-subject-selection-curriculum")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _saveSubjectSelectionCurriculumHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(DeleteSubjectSelectionCurriculum))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Subject Selection Curriculum")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(DeleteSubjectSelectionCurriculumRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> DeleteSubjectSelectionCurriculum(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route + "/delete-subject-selection-curriculum")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _deleteSubjectSelectionCurriculumHandler.Execute(req, cancellationToken);
        }


        [FunctionName(nameof(GetListSubjectSelectionCurriculumMapping))]
        [OpenApiOperation(tags: _tag, Summary = "Get List Subject Selection Curriculum Mapping")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetListSubjectSelectionCurriculumMappingRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetListSubjectSelectionCurriculumMappingRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetListSubjectSelectionCurriculumMappingResult>))]
        public Task<IActionResult> GetListSubjectSelectionCurriculumMapping(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-list-subject-selection-curriculum-mapping")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _getListSubjectSelectionCurriculumMappingHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(GetDetailSubjectSelectionCurriculumMapping))]
        [OpenApiOperation(tags: _tag, Summary = "Get Detail Subject Selection Curriculum Mapping")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetDetailSubjectSelectionCurriculumMappingRequest.CurriculumGroup), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetDetailSubjectSelectionCurriculumMappingResult))]
        public Task<IActionResult> GetDetailSubjectSelectionCurriculumMapping(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-detail-subject-selection-curriculum-mapping")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _getDetailSubjectSelectionCurriculumMappingHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(SaveSubjectSelectionCurriculumMapping))]
        [OpenApiOperation(tags: _tag, Summary = "Save Subject Selection Curriculum Mapping")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(SaveSubjectSelectionCurriculumMappingRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> SaveSubjectSelectionCurriculumMapping(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/save-subject-selection-curriculum-mapping")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _saveSubjectSelectionCurriculumMappingHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(DeleteSubjectSelectionCurriculumMapping))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Subject Selection Curriculum Mapping")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(DeleteSubjectSelectionCurriculumMappingRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> DeleteSubjectSelectionCurriculumMapping(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route + "/delete-subject-selection-curriculum-mapping")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _deleteSubjectSelectionCurriculumMappingHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(CopySubjectSelectionCurriculumMapping))]
        [OpenApiOperation(tags: _tag, Summary = "Copy Subject Selection Curriculum Mapping")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(CopySubjectSelectionCurriculumMappingRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> CopySubjectSelectionCurriculumMapping(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/copy-subject-selection-curriculum-mapping")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _copySubjectSelectionCurriculumMappingHandler.Execute(req, cancellationToken);
        }
    }
}

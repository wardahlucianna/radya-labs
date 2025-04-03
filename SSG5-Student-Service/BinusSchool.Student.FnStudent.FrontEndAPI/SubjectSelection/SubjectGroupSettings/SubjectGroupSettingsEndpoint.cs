using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalRecordEntry;
using BinusSchool.Shared.Kernel.DataModel.Student.FnStudent.SubjectSelection.SubjectGroupSettings;
using BinusSchool.Student.BLL.FnStudent.SubjectSelection.SubjectGroupSettings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BinusSchool.Student.FnStudent.FrontEndAPI.SubjectSelection.SubjectGroupSettings
{
    public class SubjectGroupSettingsEndpoint
    {
        private const string _route = "student/subject-selection/subject-group-settings";
        private const string _tags = "Subject Selection (Subject Group Settings)";

        private readonly GetSubjectGroupSettingsCurriculumHandler _getSubjectGroupSettingsCurriculumHandler;
        private readonly GetSubjectGroupSettingsCurriculumMappingHandler _getSubjectGroupSettingsCurriculumMappingHandler;
        private readonly ChangeSubjectGroupSettingsCurriculumMappingStatusHandler _changeSubjectGroupSettingsCurriculumMappingStatusHandler;
        private readonly DeleteSubjectGroupSettingsCurriculumMappingHandler _deleteSubjectGroupSettingsCurriculumMappingHandler;
        private readonly GetSubjectGroupSettingsSubjectSelectionGroupHandler _getSubjectGroupSettingsSubjectSelectionGroupHandler;
        private readonly SaveSubjectGroupSettingsSubjectSelectionGroupHandler _saveSubjectGroupSettingsSubjectSelectionGroupHandler;
        private readonly ChangeSubjectGroupSettingsSubjectSelectionGroupStatusHandler _changeSubjectGroupSettingsSubjectSelectionGroupStatusHandler;
        private readonly DeleteSubjectGroupSettingsSubjectSelectionGroupHandler _deleteSubjectGroupSettingsSubjectSelectionGroupHandler;

        public SubjectGroupSettingsEndpoint(GetSubjectGroupSettingsCurriculumHandler getSubjectGroupSettingsCurriculumHandler,
            GetSubjectGroupSettingsCurriculumMappingHandler getSubjectGroupSettingsCurriculumMappingHandler,
            ChangeSubjectGroupSettingsCurriculumMappingStatusHandler changeSubjectGroupSettingsCurriculumMappingStatusHandler,
            DeleteSubjectGroupSettingsCurriculumMappingHandler deleteSubjectGroupSettingsCurriculumMappingHandler,
            GetSubjectGroupSettingsSubjectSelectionGroupHandler getSubjectGroupSettingsSubjectSelectionGroupHandler,
            SaveSubjectGroupSettingsSubjectSelectionGroupHandler saveSubjectGroupSettingsSubjectSelectionGroupHandler,
            ChangeSubjectGroupSettingsSubjectSelectionGroupStatusHandler changeSubjectGroupSettingsSubjectSelectionGroupStatusHandler,
            DeleteSubjectGroupSettingsSubjectSelectionGroupHandler deleteSubjectGroupSettingsSubjectSelectionGroupHandler)
        {
            _getSubjectGroupSettingsCurriculumHandler = getSubjectGroupSettingsCurriculumHandler;
            _getSubjectGroupSettingsCurriculumMappingHandler = getSubjectGroupSettingsCurriculumMappingHandler;
            _changeSubjectGroupSettingsCurriculumMappingStatusHandler = changeSubjectGroupSettingsCurriculumMappingStatusHandler;
            _deleteSubjectGroupSettingsCurriculumMappingHandler = deleteSubjectGroupSettingsCurriculumMappingHandler;
            _getSubjectGroupSettingsSubjectSelectionGroupHandler = getSubjectGroupSettingsSubjectSelectionGroupHandler;
            _saveSubjectGroupSettingsSubjectSelectionGroupHandler = saveSubjectGroupSettingsSubjectSelectionGroupHandler;
            _changeSubjectGroupSettingsSubjectSelectionGroupStatusHandler = changeSubjectGroupSettingsSubjectSelectionGroupStatusHandler;
            _deleteSubjectGroupSettingsSubjectSelectionGroupHandler = deleteSubjectGroupSettingsSubjectSelectionGroupHandler;
        }

        [FunctionName(nameof(GetSubjectGroupSettingsCurriculum))]
        [OpenApiOperation(tags: _tags, Summary = "Get Subject Group Settings Curriculum")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetSubjectGroupSettingsCurriculumRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSubjectGroupSettingsCurriculumRequest.IdGrade), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSubjectGroupSettingsCurriculumRequest.Search), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetSubjectGroupSettingsCurriculumResponse>))]
        public Task<IActionResult> GetSubjectGroupSettingsCurriculum(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/curriculum")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _getSubjectGroupSettingsCurriculumHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(GetSubjectGroupSettingsCurriculumMapping))]
        [OpenApiOperation(tags: _tags, Summary = "Get Subject Group Settings Curriculum Mapping")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetSubjectGroupSettingsCurriculumMappingRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSubjectGroupSettingsCurriculumMappingRequest.IdAcademicYear), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSubjectGroupSettingsCurriculumMappingRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSubjectGroupSettingsCurriculumMappingRequest.IdCurriculum), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSubjectGroupSettingsCurriculumMappingRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSubjectGroupSettingsCurriculumMappingRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSubjectGroupSettingsCurriculumMappingRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetSubjectGroupSettingsCurriculumMappingRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetSubjectGroupSettingsCurriculumMappingRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSubjectGroupSettingsCurriculumMappingRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSubjectGroupSettingsCurriculumMappingRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetSubjectGroupSettingsCurriculumMappingResponse>))]
        public Task<IActionResult> GetSubjectGroupSettingsCurriculumMapping(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/curriculum-mapping")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _getSubjectGroupSettingsCurriculumMappingHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ChangeSubjectGroupSettingsCurriculumMappingStatus))]
        [OpenApiOperation(tags: _tags, Summary = "Change Subject Group Settings Curriculum Mapping Status")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(ChangeSubjectGroupSettingsCurriculumMappingStatusRequest), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> ChangeSubjectGroupSettingsCurriculumMappingStatus(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route + "/change-curriculum-mapping-status")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _changeSubjectGroupSettingsCurriculumMappingStatusHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(DeleteSubjectGroupSettingsCurriculumMapping))]
        [OpenApiOperation(tags: _tags, Summary = "Delete Subject Group Settings Curriculum Mapping")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(DeleteSubjectGroupSettingsCurriculumMappingRequest), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> DeleteSubjectGroupSettingsCurriculumMapping(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route + "/delete-curriculum-mapping")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _deleteSubjectGroupSettingsCurriculumMappingHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(GetSubjectGroupSettingsSubjectSelectionGroup))]
        [OpenApiOperation(tags: _tags, Summary = "Get Subject Group Settings Subject Selection Group")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetSubjectGroupSettingsSubjectSelectionGroupRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSubjectGroupSettingsSubjectSelectionGroupRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSubjectGroupSettingsSubjectSelectionGroupRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSubjectGroupSettingsSubjectSelectionGroupRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetSubjectGroupSettingsSubjectSelectionGroupRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetSubjectGroupSettingsSubjectSelectionGroupRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSubjectGroupSettingsSubjectSelectionGroupRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSubjectGroupSettingsSubjectSelectionGroupRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetSubjectGroupSettingsSubjectSelectionGroupResponse>))]
        public Task<IActionResult> GetSubjectGroupSettingsSubjectSelectionGroup(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/subject-selection-group")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _getSubjectGroupSettingsSubjectSelectionGroupHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(SaveSubjectGroupSettingsSubjectSelectionGroup))]
        [OpenApiOperation(tags: _tags, Summary = "Save Subject Group Settings Subject Selection Group")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(SaveSubjectGroupSettingsSubjectSelectionGroupRequest), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> SaveSubjectGroupSettingsSubjectSelectionGroup(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/save-subject-selection-group")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _saveSubjectGroupSettingsSubjectSelectionGroupHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ChangeSubjectGroupSettingsSubjectSelectionGroupStatus))]
        [OpenApiOperation(tags: _tags, Summary = "Change Subject Group Settings Subject Selection Group Status")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(ChangeSubjectGroupSettingsSubjectSelectionGroupStatusRequest), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> ChangeSubjectGroupSettingsSubjectSelectionGroupStatus(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route + "/change-subject-selection-group-status")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _changeSubjectGroupSettingsSubjectSelectionGroupStatusHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(DeleteSubjectGroupSettingsSubjectSelectionGroup))]
        [OpenApiOperation(tags: _tags, Summary = "Delete Subject Group Settings Subject Selection Group")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(DeleteSubjectGroupSettingsSubjectSelectionGroupRequest), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> DeleteSubjectGroupSettingsSubjectSelectionGroup(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route + "/delete-subject-selection-group")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _deleteSubjectGroupSettingsSubjectSelectionGroupHandler.Execute(req, cancellationToken);
        }
    }
}

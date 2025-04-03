using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.ServiceAsAction;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Student.FnStudent.ServiceAsAction
{
    public class ServiceAsActionEndpoint
    {
        private const string _route = "student/service-as-action";
        private const string _tag = "Service As Action";

        private readonly GetListGradeTeacherPrivilegeHandler _getListGradeTeacherPrivilege;
        private readonly GetListStudentServiceAsActionHandler _getListStudentServiceAsAction;
        private readonly GetListExperiencePerStudentHandler _GetexperiencePerStudentHandler;
        private readonly GetListLearningOutcomeForSACHandler _getListLearningOutcomeForSACHandler;  
        private readonly GetServiceAsActionDetailFormHandler _getServiceAsActionDetailFormHandler;
        private readonly GetListMappingLearningOutcomeHandler _getListMappingLearningOutcomeHandler;
        private readonly SaveExperienceServiceAsActionHandler _saveExperienceServiceAsActionHandler;
        private readonly DeleteExperienceServiceAsActionHandler _deleteExperienceServiceAsActionHandler;
        private readonly SaveServiceAsActionStatusHandler _saveServiceAsActionStatusHandler;
        private readonly SaveServiceAsActionEvidenceHandler _saveServiceAsActionEvidenceHandler;
        private readonly DeleteServiceAsActionEvidenceHandler _deleteServiceAsActionEvidenceHandler;
        private readonly SaveServiceAsActionCommentHandler _saveServiceAsActionCommentHandler;
        private readonly DeleteServiceAsActionCommentHandler _deleteServiceAsActionCommentHandler;
        private readonly SaveOverallStatusExperienceHandler _saveOverallStatusExperienceHandler;

        public ServiceAsActionEndpoint
        (
            GetListGradeTeacherPrivilegeHandler getListGradeTeacherPrivilege,
            GetListStudentServiceAsActionHandler getListStudentServiceAsAction,
            GetListExperiencePerStudentHandler GetexperiencePerStudentHandler,
            GetListLearningOutcomeForSACHandler getListLearningOutcomeForSACHandler,
            GetServiceAsActionDetailFormHandler getServiceAsActionDetailFormHandler,
            GetListMappingLearningOutcomeHandler getListMappingLearningOutcomeHandler,
            SaveExperienceServiceAsActionHandler saveExperienceServiceAsActionHandler,
            DeleteExperienceServiceAsActionHandler deleteExperienceServiceAsActionHandler,
            SaveServiceAsActionStatusHandler saveServiceAsActionStatusHandler,
            SaveServiceAsActionEvidenceHandler saveServiceAsActionEvidenceHandler,
            DeleteServiceAsActionEvidenceHandler deleteServiceAsActionEvidenceHandler,
            SaveServiceAsActionCommentHandler saveServiceAsActionCommentHandler,
            DeleteServiceAsActionCommentHandler deleteServiceAsActionCommentHandler,
            SaveOverallStatusExperienceHandler saveOverallStatusExperienceHandler
        )
        {
            _getListGradeTeacherPrivilege = getListGradeTeacherPrivilege;
            _getListStudentServiceAsAction = getListStudentServiceAsAction;
            _GetexperiencePerStudentHandler = GetexperiencePerStudentHandler;
            _getListLearningOutcomeForSACHandler = getListLearningOutcomeForSACHandler;
            _getServiceAsActionDetailFormHandler = getServiceAsActionDetailFormHandler;
            _getListMappingLearningOutcomeHandler = getListMappingLearningOutcomeHandler;
            _saveExperienceServiceAsActionHandler = saveExperienceServiceAsActionHandler;
            _deleteExperienceServiceAsActionHandler = deleteExperienceServiceAsActionHandler;
            _saveServiceAsActionStatusHandler = saveServiceAsActionStatusHandler;
            _saveServiceAsActionEvidenceHandler = saveServiceAsActionEvidenceHandler;
            _deleteServiceAsActionEvidenceHandler = deleteServiceAsActionEvidenceHandler;
            _saveServiceAsActionCommentHandler = saveServiceAsActionCommentHandler;
            _deleteServiceAsActionCommentHandler = deleteServiceAsActionCommentHandler;
            _saveOverallStatusExperienceHandler = saveOverallStatusExperienceHandler;
        }

        [FunctionName(nameof(ServiceAsActionEndpoint.GetListGradeTeacherPrivilege))]
        [OpenApiOperation(tags: _tag, Summary = "Get List Grade Teacher Privilege")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization")]
        [OpenApiParameter(nameof(GetListGradeTeacherPrivilegeRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetListGradeTeacherPrivilegeRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetListGradeTeacherPrivilegeRequest.IsAdvisor), In = ParameterLocation.Query, Type = typeof(bool), Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(IEnumerable<ItemValueVm>))]
        public Task<IActionResult> GetListGradeTeacherPrivilege(
                       [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-list-grade-teacher-privilege")] HttpRequest req,
                                  CancellationToken cancellationToken)
        {
            return _getListGradeTeacherPrivilege.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ServiceAsActionEndpoint.GetListStudentServiceAsAction))]
        [OpenApiOperation(tags: _tag, Summary = "Get List Student Service As Action")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetListStudentServiceAsActionRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetListStudentServiceAsActionRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetListStudentServiceAsActionRequest.isAdvisor), In = ParameterLocation.Query, Type = typeof(bool), Required = true)]
        [OpenApiParameter(nameof(GetListStudentServiceAsActionRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListStudentServiceAsActionRequest.IdStatus), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(IEnumerable<GetListStudentServiceAsActionResult>))]
        public Task<IActionResult> GetListStudentServiceAsAction(
                       [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-list-student-service-as-action")] HttpRequest req,
                                  CancellationToken cancellationToken)
        {
            return _getListStudentServiceAsAction.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ServiceAsActionEndpoint.GetListExperiencePerStudent))]
        [OpenApiOperation(tags: _tag, Summary = "Get List Experience Per Student")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetListExperiencePerStudentRequest.IdUser), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListExperiencePerStudentRequest.IdStudent), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetListExperiencePerStudentRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetListExperiencePerStudentRequest.IsSupervisor), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetListExperiencePerStudentResult))]
        public Task<IActionResult> GetListExperiencePerStudent(
                        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-list-experience-per-student")] HttpRequest req,
                                    CancellationToken cancellationToken)
        {
            return _GetexperiencePerStudentHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ServiceAsActionEndpoint.GetListLearningOutcomeForSAC))]
        [OpenApiOperation(tags: _tag, Summary = "Get List Learning Outcome For SAC")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetListLearningOutcomeForSACRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(IEnumerable<ItemValueVm>))]
        public Task<IActionResult> GetListLearningOutcomeForSAC(
                        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-list-learning-outcome-for-sac")] HttpRequest req,
                                    CancellationToken cancellationToken)
        {
            return _getListLearningOutcomeForSACHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ServiceAsActionEndpoint.GetServiceAsActionDetailForm))]
        [OpenApiOperation(tags: _tag, Summary = "Get Service As Action Detail Form")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetServiceAsActionDetailFormRequest.IdServiceAsActionForm), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetServiceAsActionDetailFormRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetServiceAsActionDetailFormRequest.IsIncludeComment), In = ParameterLocation.Query, Required = true, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetServiceAsActionDetailFormRequest.IsAdvisor), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetServiceAsActionDetailFormResult))]
        public Task<IActionResult> GetServiceAsActionDetailForm(
                        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-service-as-action-detail-form")] HttpRequest req,
                                     CancellationToken cancellationToken)
        {
            return _getServiceAsActionDetailFormHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ServiceAsActionEndpoint.GetListMappingLearningOutcome))]
        [OpenApiOperation(tags: _tag, Summary = "Get List Mapping Learning Outcome")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetListMappingLearningOutcomeRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(IEnumerable<ItemValueVm>))]
        public Task<IActionResult> GetListMappingLearningOutcome(
                        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-list-mapping-learning-outcome")] HttpRequest req,
                                    CancellationToken cancellationToken)
        {
            return _getListMappingLearningOutcomeHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ServiceAsActionEndpoint.SaveExperienceServiceAsAction))]
        [OpenApiOperation(tags: _tag, Summary = "Save Experience Service As Action")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(SaveExperienceServiceAsActionRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> SaveExperienceServiceAsAction(
                        [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/save-experience-service-as-action")] HttpRequest req,
                        [Queue("notification-sas")] ICollector<string> collector,
                                    CancellationToken cancellationToken)
        {
            return _saveExperienceServiceAsActionHandler.Execute(req, cancellationToken, keyValues: nameof(collector).WithValue(collector));
        }

        [FunctionName(nameof(ServiceAsActionEndpoint.DeleteExperienceServiceAsAction))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Experience Service As Action")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(DeleteExperienceServiceAsActionRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.NoContent)]
        public Task<IActionResult> DeleteExperienceServiceAsAction(
                        [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route + "/delete-experience-service-as-action")] HttpRequest req,
                                    CancellationToken cancellationToken)
        {
            return _deleteExperienceServiceAsActionHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ServiceAsActionEndpoint.SaveServiceAsActionStatus))]
        [OpenApiOperation(tags: _tag, Summary = "Save Service As Action Status")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(SaveServiceAsActionStatusRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> SaveServiceAsActionStatus(
                        [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/save-service-as-action-status")] HttpRequest req,
                                    CancellationToken cancellationToken)
        {
            return _saveServiceAsActionStatusHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ServiceAsActionEndpoint.SaveServiceAsActionEvidence))]
        [OpenApiOperation(tags: _tag, Summary = "Save Service As Action Evidence")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(SaveServiceAsActionEvidenceRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> SaveServiceAsActionEvidence(
                        [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/save-service-as-action-evidence")] HttpRequest req,
                        [Queue("notification-sas")] ICollector<string> collector, 
                                    CancellationToken cancellationToken)
        {
            return _saveServiceAsActionEvidenceHandler.Execute(req, cancellationToken, keyValues: nameof(collector).WithValue(collector));
        }

        [FunctionName(nameof(ServiceAsActionEndpoint.DeleteServiceAsActionEvidence))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Service As Action Evidence")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(DeleteServiceAsActionEvidenceRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.NoContent)]
        public Task<IActionResult> DeleteServiceAsActionEvidence(
                        [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route + "/delete-service-as-action-evidence")] HttpRequest req,
                                    CancellationToken cancellationToken)
        {
            return _deleteServiceAsActionEvidenceHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ServiceAsActionEndpoint.SaveServiceAsActionComment))]
        [OpenApiOperation(tags: _tag, Summary = "Save Service As Action Comment")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(SaveServiceAsActionCommentRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> SaveServiceAsActionComment(
                        [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/save-service-as-action-comment")] HttpRequest req,
                        [Queue("notification-sas")] ICollector<string> collector,
                                    CancellationToken cancellationToken)
        {
            return _saveServiceAsActionCommentHandler.Execute(req, cancellationToken, keyValues: nameof(collector).WithValue(collector));
        }

        [FunctionName(nameof(ServiceAsActionEndpoint.DeleteServiceAsActionComment))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Service As Action Comment")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(DeleteServiceAsActionCommentRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.NoContent)]
        public Task<IActionResult> DeleteServiceAsActionComment(
                        [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route + "/delete-service-as-action-comment")] HttpRequest req,
                                    CancellationToken cancellationToken)
        {
            return _deleteServiceAsActionCommentHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ServiceAsActionEndpoint.SaveOverallStatusExperience))]
        [OpenApiOperation(tags: _tag, Summary = "Save Overall Status Experience")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(SaveOverallStatusExperienceRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> SaveOverallStatusExperience(
                        [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/save-overall-status-experience")] HttpRequest req,
                                    CancellationToken cancellationToken)
        {
            return _saveOverallStatusExperienceHandler.Execute(req, cancellationToken);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.CreateDocumentRequest;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Document.FnDocument.DocumentRequest.CreateDocumentRequest
{
    public class CreateDocumentRequestEndPoint
    {
        private const string _route = "create-document-request";
        private const string _tag = "Create Document Request";

        private readonly GetStudentParentHomeroomInformationHandler _getStudentParentHomeroomInformationHandler;
        private readonly GetParentWithRoleByStudentHandler _getParentWithRoleByStudentHandler;
        private readonly GetDefaultPICListHandler _getDefaultPICListHandler;
        private readonly GetDocumentTypeByCategoryHandler _getDocumentTypeByCategoryHandler;
        private readonly GetStudentAYAndGradeHistoryListHandler _getStudentAYAndGradeHistoryListHandler;
        private readonly GetDocumentRequestTypeDetailAndConfigurationHandler _getDocumentRequestTypeDetailAndConfigurationHandler;
        private readonly CreateDocumentRequestStaffHandler _createDocumentRequestStaffHandler;
        private readonly CreateDocumentRequestParentHandler _createDocumentRequestParentHandler;
        private readonly UpdateDocumentRequestStaffHandler _updateDocumentRequestStaffHandler;

        public CreateDocumentRequestEndPoint(
            GetStudentParentHomeroomInformationHandler getStudentParentHomeroomInformationHandler,
            GetParentWithRoleByStudentHandler getParentWithRoleByStudentHandler,
            GetDefaultPICListHandler getDefaultPICListHandler,
            GetDocumentTypeByCategoryHandler getDocumentTypeByCategoryHandler,
            GetStudentAYAndGradeHistoryListHandler getStudentAYAndGradeHistoryListHandler,
            GetDocumentRequestTypeDetailAndConfigurationHandler getDocumentRequestTypeDetailAndConfigurationHandler,
            CreateDocumentRequestStaffHandler createDocumentRequestStaffHandler,
            CreateDocumentRequestParentHandler createDocumentRequestParentHandler,
            UpdateDocumentRequestStaffHandler updateDocumentRequestStaffHandler
            )
        {
            _getStudentParentHomeroomInformationHandler = getStudentParentHomeroomInformationHandler;
            _getParentWithRoleByStudentHandler = getParentWithRoleByStudentHandler;
            _getDefaultPICListHandler = getDefaultPICListHandler;
            _getDocumentTypeByCategoryHandler = getDocumentTypeByCategoryHandler;
            _getStudentAYAndGradeHistoryListHandler = getStudentAYAndGradeHistoryListHandler;
            _getDocumentRequestTypeDetailAndConfigurationHandler = getDocumentRequestTypeDetailAndConfigurationHandler;
            _createDocumentRequestStaffHandler = createDocumentRequestStaffHandler;
            _createDocumentRequestParentHandler = createDocumentRequestParentHandler;
            _updateDocumentRequestStaffHandler = updateDocumentRequestStaffHandler;
        }

        [FunctionName(nameof(CreateDocumentRequestEndPoint.GetStudentParentHomeroomInformation))]
        [OpenApiOperation(tags: _tag, Summary = "Get Student Parent Homeroom Information")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetStudentParentHomeroomInformationRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetStudentParentHomeroomInformationRequest.IdStudent), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetStudentParentHomeroomInformationResult))]
        public Task<IActionResult> GetStudentParentHomeroomInformation(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-student-parent-homeroom-information")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _getStudentParentHomeroomInformationHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(CreateDocumentRequestEndPoint.GetParentWithRoleByStudent))]
        [OpenApiOperation(tags: _tag, Summary = "Get Parent With Role by Student")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetParentWithRoleByStudentRequest.IdStudent), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetParentWithRoleByStudentRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetParentWithRoleByStudentRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetParentWithRoleByStudentRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetParentWithRoleByStudentRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetParentWithRoleByStudentRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetParentWithRoleByStudentRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetParentWithRoleByStudentRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetParentWithRoleByStudentResult>))]
        public Task<IActionResult> GetParentWithRoleByStudent(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-parent-with-role-by-student")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _getParentWithRoleByStudentHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(CreateDocumentRequestEndPoint.GetDefaultPICList))]
        [OpenApiOperation(tags: _tag, Summary = "Get Default PIC List")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetDefaultPICListRequest.IdDocumentReqType), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDefaultPICListRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDefaultPICListRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDefaultPICListRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetDefaultPICListRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetDefaultPICListRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDefaultPICListRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDefaultPICListRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetDefaultPICListResult>))]
        public Task<IActionResult> GetDefaultPICList(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-default-pic-list")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _getDefaultPICListHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(CreateDocumentRequestEndPoint.GetDocumentTypeByCategory))]
        [OpenApiOperation(tags: _tag, Summary = "Get Document Request Type by Category")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetDocumentTypeByCategoryRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDocumentTypeByCategoryRequest.IsAcademicDocument), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDocumentTypeByCategoryRequest.RequestByParent), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDocumentTypeByCategoryRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDocumentTypeByCategoryRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDocumentTypeByCategoryRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDocumentTypeByCategoryRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetDocumentTypeByCategoryRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetDocumentTypeByCategoryRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDocumentTypeByCategoryRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDocumentTypeByCategoryRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetDocumentTypeByCategoryResult>))]
        public Task<IActionResult> GetDocumentTypeByCategory(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-document-type-by-category")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _getDocumentTypeByCategoryHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(CreateDocumentRequestEndPoint.GetStudentAYAndGradeHistoryList))]
        [OpenApiOperation(tags: _tag, Summary = "Get Student AY and Grade History List")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetStudentAYAndGradeHistoryListRequest.IdStudent), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetStudentAYAndGradeHistoryListRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetStudentAYAndGradeHistoryListRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetStudentAYAndGradeHistoryListRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetStudentAYAndGradeHistoryListRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetStudentAYAndGradeHistoryListRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetStudentAYAndGradeHistoryListRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetStudentAYAndGradeHistoryListRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetStudentAYAndGradeHistoryListResult>))]
        public Task<IActionResult> GetStudentAYAndGradeHistoryList(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-student-ay-and-grade-history-list")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _getStudentAYAndGradeHistoryListHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(CreateDocumentRequestEndPoint.GetDocumentRequestTypeDetailAndConfiguration))]
        [OpenApiOperation(tags: _tag, Summary = "Get Document Request Type Detail and Configuration")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetDocumentRequestTypeDetailAndConfigurationRequest.IdDocumentReqType), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetDocumentRequestTypeDetailAndConfigurationResult))]
        public Task<IActionResult> GetDocumentRequestTypeDetailAndConfiguration(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-document-request-type-detail-configuration")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _getDocumentRequestTypeDetailAndConfigurationHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(CreateDocumentRequestEndPoint.CreateDocumentRequestStaff))]
        [OpenApiOperation(tags: _tag, Summary = "Create Document Request by Staff")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(CreateDocumentRequestStaffRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> CreateDocumentRequestStaff(
           [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/create-document-request-by-staff")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _createDocumentRequestStaffHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(CreateDocumentRequestEndPoint.CreateDocumentRequestParent))]
        [OpenApiOperation(tags: _tag, Summary = "Create Document Request by Parent")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(CreateDocumentRequestParentRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(CreateDocumentRequestParentResult))]
        public Task<IActionResult> CreateDocumentRequestParent(
           [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/create-document-request-by-parent")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _createDocumentRequestParentHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(CreateDocumentRequestEndPoint.UpdateDocumentRequestStaff))]
        [OpenApiOperation(tags: _tag, Summary = "Update Document Request by Staff")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateDocumentRequestStaffRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateDocumentRequestStaff(
           [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route + "/update-document-request-by-staff")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _updateDocumentRequestStaffHandler.Execute(req, cancellationToken);
        }
    }
}

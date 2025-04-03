using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.StudentInfoUpdate;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using BinusSchool.Data.Model;
using BinusSchool.Data.Model.School.FnSchool.ClassRoomMapping;

namespace BinusSchool.Student.FnStudent.StudentInfoUpdate
{
    public class StudentInfoUpdateEndPoint
    {
        private const string _route = "student/student-info-update";
        private const string _tag = "Student Info Update";

        private readonly StudentInfoUpdateHandler _studentInfoUpdateHandler;
        private readonly UpdateStudentInfoUpdateHandler _updateStudentInfoUpdateHandler;
        private readonly UpdateParentInfoUpdateHandler _updateParentInfoUpdateHandler;
        private readonly UpdateSiblingGroupInfoUpdateHandler _updateSiblingGroupInfoUpdateHandler;
        private readonly UpdateBankAccountInformationHandler _updateBankAccountInformationHandler;
        private readonly UpdateStudentPrevSchoolInfoHandler _updateStudentPrevSchoolInfoHandler;
        private readonly GetStudentInfoUpdateByClassHandler _getStudentInfoUpdateByClassHandler;
        private readonly GetDetailSummaryByClassHandler _getDetailSummaryByClassHandler;

        public StudentInfoUpdateEndPoint(StudentInfoUpdateHandler studentInfoUpdateHandler,
        UpdateStudentInfoUpdateHandler updateStudentInfoUpdateHandler,
        UpdateParentInfoUpdateHandler updateParentInfoUpdateHandler,
        UpdateSiblingGroupInfoUpdateHandler updateSiblingGroupInfoUpdateHandler,
        UpdateBankAccountInformationHandler updateBankAccountInformationHandler,
        UpdateStudentPrevSchoolInfoHandler updateStudentPrevSchoolInfoHandler,
        GetStudentInfoUpdateByClassHandler getStudentInfoUpdateByClassHandler,
        GetDetailSummaryByClassHandler getDetailSummaryByClassHandler
        )
        {
            _studentInfoUpdateHandler = studentInfoUpdateHandler;
            _updateStudentInfoUpdateHandler = updateStudentInfoUpdateHandler;
            _updateParentInfoUpdateHandler = updateParentInfoUpdateHandler;
            _updateSiblingGroupInfoUpdateHandler = updateSiblingGroupInfoUpdateHandler;
            _updateBankAccountInformationHandler = updateBankAccountInformationHandler;
            _updateStudentPrevSchoolInfoHandler = updateStudentPrevSchoolInfoHandler;
            _getStudentInfoUpdateByClassHandler = getStudentInfoUpdateByClassHandler;
            _getDetailSummaryByClassHandler = getDetailSummaryByClassHandler;
        }

        [FunctionName(nameof(StudentInfoUpdateEndPoint.GetStudentInfoUpdates))]
        [OpenApiOperation(tags: _tag, Summary = "Get Student Info Update List")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(CollectionRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetStudentInfoUpdateRequest.IsParentUpdate), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetStudentInfoUpdateRequest.IdApprovalStatus), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetStudentInfoUpdateResult))]
        public Task<IActionResult> GetStudentInfoUpdates(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _studentInfoUpdateHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(StudentInfoUpdateEndPoint.UpdateStudentInfoUpdate))]
        [OpenApiOperation(tags: _tag, Summary = "Update Student Info")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(IEnumerable<UpdateStudentInfoUpdate>))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateStudentInfoUpdate(
           [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route + "-save")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _updateStudentInfoUpdateHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(StudentInfoUpdateEndPoint.UpdateParentInfoUpdate))]
        [OpenApiOperation(tags: _tag, Summary = "Update Parent Info")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(IEnumerable<UpdateStudentInfoUpdate>))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateParentInfoUpdate(
           [HttpTrigger(AuthorizationLevel.Function, "put", Route = "student/parent-info-update-save")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _updateParentInfoUpdateHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(StudentInfoUpdateEndPoint.UpdateSiblingGroupInfoUpdate))]
        [OpenApiOperation(tags: _tag, Summary = "Update Sibling Group Info")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(IEnumerable<UpdateStudentInfoUpdate>))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateSiblingGroupInfoUpdate(
           [HttpTrigger(AuthorizationLevel.Function, "put", Route = "student/siblinggroup-info-update-save")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _updateSiblingGroupInfoUpdateHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(StudentInfoUpdateEndPoint.UpdateBankAccountInformationInfoUpdate))]
        [OpenApiOperation(tags: _tag, Summary = "Update Bank Account Info")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(IEnumerable<UpdateStudentInfoUpdate>))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateBankAccountInformationInfoUpdate(
           [HttpTrigger(AuthorizationLevel.Function, "put", Route = "student/bank-account-info-update-save")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _updateBankAccountInformationHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(StudentInfoUpdateEndPoint.UpdateStudentPrevSchoolInfoUpdate))]
        [OpenApiOperation(tags: _tag, Summary = "Update Student Prev School Info")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(IEnumerable<UpdateStudentInfoUpdate>))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateStudentPrevSchoolInfoUpdate(
          [HttpTrigger(AuthorizationLevel.Function, "put", Route = "student/prevschool-info-update-save")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            return _updateStudentPrevSchoolInfoHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(StudentInfoUpdateEndPoint.GetStudentInfoUpdateByClass))]
        [OpenApiOperation(tags: _tag, Summary = "Get Student Info Update By Class")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetMappingClassByLevelRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetMappingClassByLevelRequest.IdAcadyear), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetMappingClassByLevelRequest.IdLevel), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetMappingClassByLevelRequest.Search), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetStudentInfoUpdateApprovalResult))]
        public Task<IActionResult> GetStudentInfoUpdateByClass(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = "student/get-student-info-update-By-class")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _getStudentInfoUpdateByClassHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(StudentInfoUpdateEndPoint.GetDetailSummaryByClass))]
        [OpenApiOperation(tags: _tag, Summary = "Get Detail Summary By Class")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetDetailApprovalSummaryRequest.IdGrade), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDetailApprovalSummaryRequest.IdClass), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDetailApprovalSummaryRequest.IsParentUpdate), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetDetailApprovalSummaryResult))]
        public Task<IActionResult> GetDetailSummaryByClass(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = "student/get-detail-summary-By-class")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _getDetailSummaryByClassHandler.Execute(req, cancellationToken);
        }
    }
}

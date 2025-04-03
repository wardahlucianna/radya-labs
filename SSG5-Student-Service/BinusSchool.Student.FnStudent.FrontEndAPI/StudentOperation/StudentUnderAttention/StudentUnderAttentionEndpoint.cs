using BinusSchool.Student.BLL.FnStudent.StudentOperation.StudentUnderAttention;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Azure.WebJobs;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Shared.Kernel.DataModel.Student.FnStudent.StudentOperation.StudentUnderAttention;
using BinusSchool.Student.BLL.FnStudent.StudentStatus.SendEmail;
using BinusSchool.Shared.Kernel.DataModel.Student.FnStudent.StudentStatus.SendEmail;

namespace BinusSchool.Student.FnStudent.FrontEndAPI.StudentOperation.StudentUnderAttention
{
    public class StudentUnderAttentionEndpoint
    {
        private const string _route = "student/student-operation/student-under-attention";
        private const string _tags = "Student Operation (Student Under Attention)";

        private readonly GetStudentUnderAttentionStudentStatusSpecialHandler _getStudentUnderAttentionStudentStatusSpecialHandler;
        private readonly GetStudentUnderAttentionHandler _getStudentUnderAttentionStudentHandler;
        private readonly GetStudentUnderAttentionFutureAdmissionDecisionFormHandler _getStudentUnderAttentionFutureAdmissionDecisionFormHandler;
        private readonly SaveStudentUnderAttentionFutureAdmissionDecisionFormHandler _saveStudentUnderAttentionFutureAdmissionDecisionFormHandler;

        private readonly SendEmailStudentStatusSpecialNeedFutureAdmission sendEmailStudentStatusSpecialNeedFutureAdmission;

        public StudentUnderAttentionEndpoint(GetStudentUnderAttentionStudentStatusSpecialHandler getStudentUnderAttentionStudentStatusSpecialHandler, GetStudentUnderAttentionHandler getStudentUnderAttentionStudentHandler, GetStudentUnderAttentionFutureAdmissionDecisionFormHandler getStudentUnderAttentionFutureAdmissionDecisionFormHandler, SaveStudentUnderAttentionFutureAdmissionDecisionFormHandler saveStudentUnderAttentionFutureAdmissionDecisionFormHandler, SendEmailStudentStatusSpecialNeedFutureAdmission sendEmailStudentStatusSpecialNeedFutureAdmission)
        {
            _getStudentUnderAttentionStudentStatusSpecialHandler = getStudentUnderAttentionStudentStatusSpecialHandler;
            _getStudentUnderAttentionStudentHandler = getStudentUnderAttentionStudentHandler;
            _getStudentUnderAttentionFutureAdmissionDecisionFormHandler = getStudentUnderAttentionFutureAdmissionDecisionFormHandler;
            _saveStudentUnderAttentionFutureAdmissionDecisionFormHandler = saveStudentUnderAttentionFutureAdmissionDecisionFormHandler;
            this.sendEmailStudentStatusSpecialNeedFutureAdmission = sendEmailStudentStatusSpecialNeedFutureAdmission;
        }

        [FunctionName(nameof(GetStudentUnderAttentionStudentStatusSpecial))]
        [OpenApiOperation(tags: _tags, Summary = "Get Student Under Attention Student Status Special")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetStudentUnderAttentionStudentStatusSpecialResponse>))]
        public Task<IActionResult> GetStudentUnderAttentionStudentStatusSpecial(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/student-status-special")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _getStudentUnderAttentionStudentStatusSpecialHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(GetStudentUnderAttention))]
        [OpenApiOperation(tags: _tags, Summary = "Get Student Under Attention")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetStudentUnderAttentionRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetStudentUnderAttentionRequest.IdAcademicYear), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetStudentUnderAttentionRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetStudentUnderAttentionRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetStudentUnderAttentionRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetStudentUnderAttentionRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetStudentUnderAttentionRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetStudentUnderAttentionRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetStudentUnderAttentionRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetStudentUnderAttentionResponse>))]
        public Task<IActionResult> GetStudentUnderAttention(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req, CancellationToken cancellationToken)
        {
            return _getStudentUnderAttentionStudentHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(GetStudentUnderAttentionFutureAdmissionDecisionForm))]
        [OpenApiOperation(tags: _tags, Summary = "Get Student Under Attention Future Admission Decision Form")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetStudentUnderAttentionFutureAdmissionDecisionFormRequest.IdTrStudentStatus), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetStudentUnderAttentionFutureAdmissionDecisionFormResponse))]
        public Task<IActionResult> GetStudentUnderAttentionFutureAdmissionDecisionForm(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/future-admission-decision-form")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _getStudentUnderAttentionFutureAdmissionDecisionFormHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(SaveStudentUnderAttentionFutureAdmissionDecisionForm))]
        [OpenApiOperation(tags: _tags, Summary = "Save Student Under Attention Future Admission Decision Form")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(SaveStudentUnderAttentionFutureAdmissionDecisionFormRequest), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> SaveStudentUnderAttentionFutureAdmissionDecisionForm(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/save-future-admission-decision-form")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _saveStudentUnderAttentionFutureAdmissionDecisionFormHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(SendEmailStudentStatusSpecialNeedFutureAdmission))]
        [OpenApiOperation(tags: _tags, Summary = "Send Email Student Status Special Need Future Admission")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(SendEmailStudentStatusSpecialNeedFutureAdmissionRequest), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> SendEmailStudentStatusSpecialNeedFutureAdmission(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/send-email/need-future-admission")] HttpRequest req, CancellationToken cancellationToken)
        {
            return sendEmailStudentStatusSpecialNeedFutureAdmission.Execute(req, cancellationToken);
        }
    }
}

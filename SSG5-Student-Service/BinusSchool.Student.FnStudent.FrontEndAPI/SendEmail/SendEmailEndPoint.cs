using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.SendEmail;
using BinusSchool.Data.Model.Student.FnStudent.StudentDocument;
using BinusSchool.Student.FnStudent.Student;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Student.FnStudent.SendEmail
{
    public class SendEmailEndPoint
    {
        private const string _route = "student/send-email";
        private const string _tag = "Send Email Student Profile Notification";

        private readonly SendEmailProfileUpdateToStaffHandler _sendEmailProfileUpdateToStaffHandler;
        private readonly SendEmailProfileApprovalUpdateToParentHandler _sendEmailProfileApprovalUpdateToParentHandler;

        public SendEmailEndPoint(
            SendEmailProfileUpdateToStaffHandler sendEmailProfileUpdateToStaffHandler,
            SendEmailProfileApprovalUpdateToParentHandler sendEmailProfileApprovalUpdateToParentHandler)
        {
            _sendEmailProfileUpdateToStaffHandler = sendEmailProfileUpdateToStaffHandler;
            _sendEmailProfileApprovalUpdateToParentHandler = sendEmailProfileApprovalUpdateToParentHandler;
        }

        [FunctionName(nameof(SendEmailEndPoint.SendEmailProfileUpdateToStaff))]
        [OpenApiOperation(tags: _tag, Summary = "Send Email Profile Update To Staff")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(SendEmailProfileUpdateToStaffRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> SendEmailProfileUpdateToStaff(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/profile-update-to-staff")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _sendEmailProfileUpdateToStaffHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(SendEmailEndPoint.SendEmailProfileApprovalUpdateToParent))]
        [OpenApiOperation(tags: _tag, Summary = "Send Email Profile Approval Update To Parent")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(SendEmailProfileApprovalUpdateToParentRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> SendEmailProfileApprovalUpdateToParent(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/approval-profile-update-to-parent")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _sendEmailProfileApprovalUpdateToParentHandler.Execute(req, cancellationToken);
        }
    }
}

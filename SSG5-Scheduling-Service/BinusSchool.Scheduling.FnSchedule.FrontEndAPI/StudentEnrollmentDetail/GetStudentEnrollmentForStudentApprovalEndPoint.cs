using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Data.Model.Scheduling.FnSchedule.StudentEnrollment;
using BinusSchool.Data.Model.Scheduling.FnSchedule.StudentEnrollmentDetail;
using BinusSchool.Scheduling.FnSchedule.StudentEnrollment;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Scheduling.FnSchedule.StudentEnrollmentDetail
{
    public class GetStudentEnrollmentForStudentApprovalEndPoint
    {
        private const string _route = "schedule/student-enrollment-detail";
        private const string _tag = "Student Enrollment Detail";

        private readonly GetStudentEnrollmentForStudentApprovalSummaryHandler _getStudentEnrollmentForStudentApprovalSummaryHandler;

        public GetStudentEnrollmentForStudentApprovalEndPoint(GetStudentEnrollmentForStudentApprovalSummaryHandler getStudentEnrollmentForStudentApprovalSummaryHandler)
        {
            _getStudentEnrollmentForStudentApprovalSummaryHandler = getStudentEnrollmentForStudentApprovalSummaryHandler;
        }


        [FunctionName(nameof(GetStudentEnrollmentForStudentApprovalEndPoint.GetStudentEnrollmentForStudentApprovalSummary))]
        [OpenApiOperation(tags: _tag, Summary = "Get Student list")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(GetStudentEnrollmentforStudentApprovalSummaryRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetStudentEnrollmentforStudentApprovalSummaryResult))]
        public Task<IActionResult> GetStudentEnrollmentForStudentApprovalSummary(
           [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/get-list-student")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _getStudentEnrollmentForStudentApprovalSummaryHandler.Execute(req, cancellationToken,false);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Azure.WebJobs;
using Microsoft.OpenApi.Models;
using System.Threading.Tasks;
using System.Threading;
using BinusSchool.Data.Model.Teaching.FnAssignment.TeacherAssignmentHistory;

namespace BinusSchool.Teaching.FnAssignment.TeacherAssignmentHistory
{
    public class TeacherAssignmentHistoryEndpoint
    {
        private const string _route = "teaching/teacher-assignment-history";
        private const string _tag = "Teacher Assignment History";

        private readonly GetTeacherAssignmentHistoryHandler _teacherAssignmentHistoryHandler;
        public TeacherAssignmentHistoryEndpoint(GetTeacherAssignmentHistoryHandler teacherAssignmentHistoryHandler)
        {
            _teacherAssignmentHistoryHandler = teacherAssignmentHistoryHandler;
        }

        [FunctionName(nameof(GetTeacherAssignmentHistory))]
        [OpenApiOperation(tags: _tag, Summary = "Get Teacher Assignment Transaction")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(GetTeacherAssignmentHistoryRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetTeacherAssignmentHistoryResult>))]
        public Task<IActionResult> GetTeacherAssignmentHistory(
           [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route)] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _teacherAssignmentHistoryHandler.Execute(req, cancellationToken);
        }
    }
}

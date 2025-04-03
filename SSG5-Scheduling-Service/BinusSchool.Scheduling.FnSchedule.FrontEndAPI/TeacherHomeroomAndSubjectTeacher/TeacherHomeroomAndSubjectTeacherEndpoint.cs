using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Data.Model.Scheduling.FnSchedule.TeacherHomeroomAndSubjectTeacher;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Scheduling.FnSchedule.TeacherHomeroomAndSubjectTeacher
{
    public class TeacherHomeroomAndSubjectTeacherEndpoint
    {
        private const string _route = "schedule/teacher-assignment";
        private const string _tag = "Teacher Assignment";

        private readonly TeacherHomeroomAndSubjectTeacherHandler _homeroomAndSubjectTeacherHandler;

        public TeacherHomeroomAndSubjectTeacherEndpoint(TeacherHomeroomAndSubjectTeacherHandler homeroomAndSubjectTeacherHandler)
        {
            _homeroomAndSubjectTeacherHandler = homeroomAndSubjectTeacherHandler;
        }

        [FunctionName(nameof(TeacherHomeroomAndSubjectTeacherEndpoint.GetTeacherAssignment))]
        [OpenApiOperation(tags: _tag, Summary = "Teacher Assignment")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(TeacherHomeroomAndSubjectTeacherRequest.IdUser), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(TeacherHomeroomAndSubjectTeacherRequest.IdSchool), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<TeacherHomeroomAndSubjectTeacherRequest>))]
        public Task<IActionResult> GetTeacherAssignment(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _homeroomAndSubjectTeacherHandler.Execute(req, cancellationToken, false);
        }
    }
}

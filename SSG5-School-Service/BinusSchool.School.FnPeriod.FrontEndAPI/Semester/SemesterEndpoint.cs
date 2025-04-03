using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Data.Model.School.FnPeriod.Semester;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace BinusSchool.School.FnPeriod.Semester
{
    public class SemesterEndpoint
    {
        private const string _route = "school/semester";
        private const string _tag = "School Semester"; 

        private readonly SemesterHandler _handler;

        public SemesterEndpoint(SemesterHandler handler)
        {
            _handler = handler;
        }

        [FunctionName(nameof(SemesterEndpoint.GetSemesters))]
        [OpenApiOperation(tags: _tag, Summary = "Get List Semester")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetSemesterRequest.IdGrade), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(int[]))]
        public Task<IActionResult> GetSemesters(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _handler.Execute(req, cancellationToken);
        }
    }
}
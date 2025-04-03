using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Data.Model.School.FnSchool.GetActiveAcademicYear;
using BinusSchool.Data.Model.School.FnSchool.GetActiveSemester;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.School.FnSchool.GetActiveSemester
{
    public class GetActiveSemesterEndPoint
    {
        private const string _route = "school/Semester";
        private const string _tag = "Get School Active Semester";

        [FunctionName(nameof(GetActiveSemesterEndPoint.GetActiveSemester))]
        [OpenApiOperation(tags: _tag, Summary = "Get Field Data List")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(GetActiveSemesterRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetActiveSemesterResult))]
        public Task<IActionResult> GetActiveSemester(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/GetActiveSemester")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetActiveSemesterHandler>();
            return handler.Execute(req, cancellationToken, false);
        }
    }
}

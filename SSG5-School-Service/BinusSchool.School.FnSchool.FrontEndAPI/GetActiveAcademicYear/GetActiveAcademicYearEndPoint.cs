using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Data.Model.School.FnSchool.GetActiveAcademicYear;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.School.FnSchool.GetActiveAcademicYear
{
    public class GetActiveAcademicYearEndPoint
    {
        private const string _route = "school/academicyears";
        private const string _tag = "Get School Active Academic year";

        [FunctionName(nameof(GetActiveAcademicYearEndPoint.GetActiveAcademicYear))]
        [OpenApiOperation(tags: _tag, Summary = "Get Field Data List")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(GetActiveAcademicYearRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetActiveAcademicYearResult))]
        public Task<IActionResult> GetActiveAcademicYear(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/GetActiveAcademicYear")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetActiveAcademicYearHandler>();
            return handler.Execute(req, cancellationToken, false);
        }
    }
}

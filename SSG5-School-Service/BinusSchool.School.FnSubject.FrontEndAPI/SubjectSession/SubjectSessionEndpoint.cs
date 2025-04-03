using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Data.Model.School.FnSubject.SubjectSession;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.School.FnSubject.SubjectSession
{
    public class SubjectSessionEndpoint
    {
        private const string _route = "school/subject-session";
        private const string _tag = "School Subject Session";

        [FunctionName(nameof(GetSubjectSession))]
        [OpenApiOperation(tags: _tag, Summary = "Get Subject Combination")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetSubjectSessionRequest.IdSubject), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetSubjectSessionResult[]))]
        public Task<IActionResult> GetSubjectSession(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req, 
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetSubjectSessionHandler>();
            return handler.Execute(req, cancellationToken);
        }
    }
}

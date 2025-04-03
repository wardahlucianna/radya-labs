using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Student.FnStudent.LastEducationLevel
{
    public class LastEducationLevelEndPoint
    {
        private const string _route = "student/last-education-level";
        private const string _tag = "Last Education Level";

        private readonly LastEducationLevelHandler _lastEducationLevelHandler;
        public LastEducationLevelEndPoint(LastEducationLevelHandler lastEducationLevelHandler)
        {
            _lastEducationLevelHandler = lastEducationLevelHandler;
        }

        [FunctionName(nameof(LastEducationLevelEndPoint.GetLastEducationLevel))]
        [OpenApiOperation(tags: _tag, Summary = "Get Last Education Level List")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(ItemValueVm))]
        public Task<IActionResult> GetLastEducationLevel(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _lastEducationLevelHandler.Execute(req, cancellationToken);
        }
        
    }
}

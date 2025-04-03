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

namespace BinusSchool.Student.FnStudent.AliveStatus
{
    public class AliveStatusEndPoint
    {
        private const string _route = "student/alive-status";
        private const string _tag = "Literal table";

         private readonly AliveStatusHandler _aliveStatusHandler;
        public AliveStatusEndPoint(AliveStatusHandler aliveStatusHandler)
        {
            _aliveStatusHandler = aliveStatusHandler;
        }

         [FunctionName(nameof(AliveStatusEndPoint.GetAliveStatus))]
        [OpenApiOperation(tags: _tag, Summary = "Get Alive Status List")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(ItemValueVm))]
        public Task<IActionResult> GetAliveStatus(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _aliveStatusHandler.Execute(req, cancellationToken);
        }
    }
}

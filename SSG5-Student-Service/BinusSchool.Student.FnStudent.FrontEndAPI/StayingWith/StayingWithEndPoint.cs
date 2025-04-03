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

namespace BinusSchool.Student.FnStudent.StayingWith
{
    public class StayingWithEndPoint
    {
        private const string _route = "student/staying-with";
        private const string _tag = "Literal table";

         private readonly StayingWithHandler _stayingWithHandler;
        public StayingWithEndPoint(StayingWithHandler stayingWithHandler)
        {
            _stayingWithHandler = stayingWithHandler;
        }

        [FunctionName(nameof(StayingWithEndPoint.GetStayingWith))]
        [OpenApiOperation(tags: _tag, Summary = "Get Staying With List")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(ItemValueVm))]
        public Task<IActionResult> GetStayingWith(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _stayingWithHandler.Execute(req, cancellationToken);
        }

    }
}

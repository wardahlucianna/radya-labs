using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Data.Model.Attendance.FnAttendance.QuotaV2;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Attendance.FnAttendance.QuotaV2
{
    public class QuotaV2Endpoint
    {
        private const string _route = "quotaV2";
        private const string _tag = "Quota V2";

        [FunctionName(nameof(QuotaV2Endpoint.GetQuotaV2Detail))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter("idLevel", In = ParameterLocation.Path, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(QuotaV2Result))]
        public Task<IActionResult> GetQuotaV2Detail(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/detail/{idLevel}")] HttpRequest req,
            string idLevel,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetQuotaV2DetailHandler>();
            return handler.Execute(req, cancellationToken, keyValues: "idLevel".WithValue(idLevel));
        }

        [FunctionName(nameof(QuotaV2Endpoint.SetQuotaV2))]
        [OpenApiOperation(tags: _tag, Summary = "Set QuotaV2")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(SetQuotaV2Request))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> SetQuotaV2(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<SetQuotaV2Handler>();
            return handler.Execute(req, cancellationToken);
        }
    }
}

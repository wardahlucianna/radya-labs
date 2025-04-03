using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Data.Model.Util.FnConverter.BnsReportToPdf;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Util.FnConverter.BnsReportToPdf
{
    public class BnsReportToPdfEndPoint
    {
        private const string _route = "bnsReport-to-pdf";
        private const string _tag = "Bns Report To Pdf";

        [FunctionName(nameof(BnsReportToPdfEndPoint.ConvertBnsReportToPdf))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(ConvertBnsReportToPdfRequest))]
        [OpenApiResponseWithBody(System.Net.HttpStatusCode.OK, "application/json", typeof(ConvertBnsReportToPdfResult))]
        public Task<IActionResult> ConvertBnsReportToPdf(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<ConvertBnsReportToPdfHandler>();
            return handler.Execute(req, cancellationToken, false);
        }
    }
}

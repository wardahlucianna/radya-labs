using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Data.Model.Util.FnConverter.HtmlToPdf;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Util.FnConverter.HtmlToPdf
{
    public class HtmlToPdfEndpoint
    {
        private const string _route = "html-to-pdf";
        private const string _tag = "Convert Html to Pdf";

        [FunctionName(nameof(HtmlToPdfEndpoint.ConvertHtmlToPdf))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        // [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(ConvertHtmlToPdfRequest))]
        [OpenApiResponseWithBody(System.Net.HttpStatusCode.OK, "application/json", typeof(FileContentResult))]
        public Task<IActionResult> ConvertHtmlToPdf(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<ConvertHtmlToPdfHandler>();
            return handler.Execute(req, cancellationToken, false);
        }
    }
}

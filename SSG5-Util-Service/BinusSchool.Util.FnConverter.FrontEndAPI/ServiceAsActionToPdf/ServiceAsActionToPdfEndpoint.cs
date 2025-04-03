using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Data.Model.Util.FnConverter.ServiceAsActionToPdf;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Util.FnConverter.ServiceAsActionToPdf
{
    public class ServiceAsActionToPdfEndpoint
    {
        private readonly ConvertServiceAsActionToPdfHandler _convertServiceAsActionToPdfHandler;

        public ServiceAsActionToPdfEndpoint
        (
            ConvertServiceAsActionToPdfHandler convertServiceAsActionToPdfHandler
        )
        {
            _convertServiceAsActionToPdfHandler = convertServiceAsActionToPdfHandler;
        }

        private const string _route = "service-as-action-to-pdf";
        private const string _tag = "Convert Service As Action to Pdf";

        [FunctionName(nameof(ServiceAsActionToPdfEndpoint.ConvertServiceAsActionToPdf))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(ConvertServiceAsActionToPdfRequest))]
        [OpenApiResponseWithBody(System.Net.HttpStatusCode.OK, "application/json", typeof(FileContentResult))]
        public Task<IActionResult> ConvertServiceAsActionToPdf(
                       [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route)] HttpRequest req,
                                  CancellationToken cancellationToken)
        {
            return _convertServiceAsActionToPdfHandler.Execute(req, cancellationToken, false);
        }
    }
}

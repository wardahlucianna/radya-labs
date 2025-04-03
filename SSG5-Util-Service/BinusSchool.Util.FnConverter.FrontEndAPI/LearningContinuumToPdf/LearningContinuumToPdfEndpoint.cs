using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Util.FnConverter.HtmlToPdf;
using BinusSchool.Util.FnConverter.HtmlToPdf;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using BinusSchool.Data.Model.Util.FnConverter.LearningContinuumToPdf;

namespace BinusSchool.Util.FnConverter.LearningContinuumToPdf
{
    public class LearningContinuumToPdfEndpoint
    {
        private readonly ConvertLearningContinuumToPdfHandler _convertLearningContinuumToPdfHandler;

        public LearningContinuumToPdfEndpoint
        (
            ConvertLearningContinuumToPdfHandler convertLearningContinuumToPdfHandler
        )
        {
            _convertLearningContinuumToPdfHandler = convertLearningContinuumToPdfHandler;
        }

        private const string _route = "learning-continuum-to-pdf";
        private const string _tag = "Convert Learning Continuum to Pdf";

        [FunctionName(nameof(LearningContinuumToPdfEndpoint.ConvertLearningContinuumToPdf))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(ConvertLearningContinuumToPdfRequest))]
        [OpenApiResponseWithBody(System.Net.HttpStatusCode.OK, "application/json", typeof(FileContentResult))]
        public Task<IActionResult> ConvertLearningContinuumToPdf(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _convertLearningContinuumToPdfHandler.Execute(req, cancellationToken, false);
        }

    }
}

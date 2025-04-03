using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Azure.WebJobs;
using Microsoft.OpenApi.Models;
using BinusSchool.Data.Model.School.FnSchool.MobileVersion;
using Microsoft.Extensions.DependencyInjection;

namespace BinusSchool.School.FnSchool.MobileVersion
{
    public class MobileVersionEndPoint
    {
        private const string _route = "school/mobile-version";
        private const string _tag = "Mobile Version";

        [FunctionName(nameof(MobileVersionEndPoint.GetMobileVersion))]
        [OpenApiOperation(tags: _tag, Summary = "Get Mobile Version")]
        //[OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        //[OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(MobileVersionRequest.OperatingSystem), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(MobileVersionResult))]
        public Task<IActionResult> GetMobileVersion(
         [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "school/mobile-version")] HttpRequest req,
         CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<MobileVersionHandler>();
            return handler.Execute(req, cancellationToken, false);
        }
    }
}

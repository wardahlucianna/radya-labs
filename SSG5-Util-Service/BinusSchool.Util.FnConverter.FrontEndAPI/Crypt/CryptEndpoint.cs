using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Data.Model.Util.FnConverter.Crypt;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Util.FnConverter.Crypt
{
    public class CryptEndpoint
    {
        private const string _route = "crypt";
        private const string _tag = "Crypt Rijndael";

        [FunctionName(nameof(CryptEndpoint.GetRijndael))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(RijndaelRequest.Type), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(RijndaelRequest.Name), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(RijndaelRequest.Key), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(RijndaelResult))]
        public Task<IActionResult> GetRijndael(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-rijndael-crypt")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<RijndaelHandler>();
            return handler.Execute(req, cancellationToken, false);
        }


        [FunctionName(nameof(CryptEndpoint.GetSHA1))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(Sha1Request.Name), In = ParameterLocation.Query, Required = true)]      
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(Sha1Result))]
        public Task<IActionResult> GetSHA1(
         [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-sha1-crypt")] HttpRequest req,
         CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<Sha1Handler>();
            return handler.Execute(req, cancellationToken, false);
        }
    }
}

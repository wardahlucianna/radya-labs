using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.MasterExtracurricularSession;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Scheduling.FnExtracurricular.MasterExtracurricularSession
{
    public class MasterExtracurricularSessionEndPoint
    {

        private const string _route = "extracurricular/master-session";
        private const string _tag = "Master Extracurricular Session";

        [FunctionName(nameof(MasterExtracurricularSessionEndPoint.CheckAvailableSession))]
        [OpenApiOperation(tags: _tag, Summary = "Check Available Master Extracurricular Session")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(CheckAvailableSessionForExtracurricularRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(CheckAvailableSessionForExtracurricularResult))]
        public Task<IActionResult> CheckAvailableSession(
           [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/CheckAvailableSession")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<CheckAvailableSessionForExtracurricularHandler>();
            return handler.Execute(req, cancellationToken);
        }

    }
}

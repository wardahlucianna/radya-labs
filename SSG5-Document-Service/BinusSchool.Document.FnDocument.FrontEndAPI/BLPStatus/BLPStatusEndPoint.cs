using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Data.Model.Document.FnDocument.BLPStatus;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Document.FnDocument.BLPStatus
{
    public class BLPStatusEndPoint
    {
        private const string _route = "blp/blp-status";
        private const string _tag = "Blended Learning Program Status";

        private readonly BLPStatusHandler _bLPGroupHandler;

        public BLPStatusEndPoint(BLPStatusHandler bLPGroupHandler)
        {
            _bLPGroupHandler = bLPGroupHandler;
        }

        [FunctionName(nameof(BLPStatusEndPoint.GetBLPStatus))]
        [OpenApiOperation(tags: _tag, Summary = "Get BLP Status")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetBLPStatusRequest.IdSchool), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetBLPStatusResult>))]
        public Task<IActionResult> GetBLPStatus([HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _bLPGroupHandler.Execute(req, cancellationToken);
        }
    }
}

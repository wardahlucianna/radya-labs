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

namespace BinusSchool.Student.FnStudent.BinusianStatus
{
    public class BinusianStatusEndPoint
    {
        private const string _route = "student/binusian-status";
        private const string _tag = "Literal table";
     
        private readonly BinusianStatusHandler _binusianStatusHandler;
        public BinusianStatusEndPoint(BinusianStatusHandler binusianStatusHandler)
        {
            _binusianStatusHandler = binusianStatusHandler;
        }
        
        [FunctionName(nameof(BinusianStatusEndPoint.GetBinusianStatus))]
        [OpenApiOperation(tags: _tag, Summary = "Get Binusian Status List")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(ItemValueVm))]
        public Task<IActionResult> GetBinusianStatus(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _binusianStatusHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(BinusianStatusEndPoint.GetBinusianStatusDetail))]
        [OpenApiOperation(tags: _tag, Summary = "Get Binusian Status Detail")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter("id", Required = true)]
        //[OpenApiParameter(nameof(GetSiblingGroupRequest.IdStudent), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(ItemValueVm))]
        public Task<IActionResult> GetBinusianStatusDetail(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/{id}")] HttpRequest req,
           string id,
           CancellationToken cancellationToken)
        {
            return _binusianStatusHandler.Execute(req, cancellationToken, true, "id".WithValue(id));
        }
    }
}

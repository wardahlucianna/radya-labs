using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.ElectivesObjective;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Scheduling.FnExtracurricular.ElectivesObjective
{
    public class ElectivesObjectiveEndpoint
    {
        private const string _route = "elective/elective-objective";
        private const string _tag = "Elective Objective";

        private readonly ElectivesObjectiveHandler _electivesObjectiveHandler;

        public ElectivesObjectiveEndpoint(
        ElectivesObjectiveHandler electivesObjectiveHandler)
        {
            _electivesObjectiveHandler = electivesObjectiveHandler;
        }

        [FunctionName(nameof(ElectivesObjectiveEndpoint.DeleteElectivesObjective))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Electives Objective")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(IEnumerable<string>), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> DeleteElectivesObjective(
        [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route)] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _electivesObjectiveHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ElectivesObjectiveEndpoint.GetElectivesObjective))]
        [OpenApiOperation(tags: _tag, Summary = "Get Electives Objective")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetElectivesObjectiveRequest.IdExtracurricular), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetElectivesObjectiveResult>))]
        public Task<IActionResult> GetElectivesObjective([HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _electivesObjectiveHandler.Execute(req, cancellationToken);
        }


        [FunctionName(nameof(ElectivesObjectiveEndpoint.AddElectivesObjective))]
        [OpenApiOperation(tags: _tag, Summary = "Add Elective Objective")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddElectivesObjectiveRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> AddElectivesObjective(
         [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route)] HttpRequest req,
         CancellationToken cancellationToken)
        {
            return _electivesObjectiveHandler.Execute(req, cancellationToken);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.ExtracurricularScoreComponent;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Scheduling.FnExtracurricular.ExtracurricularScoreComponent
{
    public class ExtracurricularScoreComponentEndPoint
    {
        private const string _route = "extracurricular/score-component";
        private const string _tag = "Extracurricular Score Component";

        private readonly ExtracurricularScoreComponentHandler _extracurricularScoreComponentHandler;
        private readonly ExtracurricularScoreComponentHandler2 _extracurricularScoreComponentHandler2;
        private readonly GetExtracurricularScoreCalculationTypeHandler _getExtracurricularScoreCalculationTypeHandler;

        public ExtracurricularScoreComponentEndPoint(
        ExtracurricularScoreComponentHandler extracurricularScoreComponentHandler,
            ExtracurricularScoreComponentHandler2 extracurricularScoreComponentHandler2,
            GetExtracurricularScoreCalculationTypeHandler getExtracurricularScoreCalculationTypeHandler)
        {
            _extracurricularScoreComponentHandler = extracurricularScoreComponentHandler;
            _extracurricularScoreComponentHandler2 = extracurricularScoreComponentHandler2;
            _getExtracurricularScoreCalculationTypeHandler = getExtracurricularScoreCalculationTypeHandler;
        }

        [FunctionName(nameof(ExtracurricularScoreComponentEndPoint.DeleteExtracurricularScoreComponent))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Extracurricular Score Component")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(IEnumerable<string>), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> DeleteExtracurricularScoreComponent(
       [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route)] HttpRequest req,
       CancellationToken cancellationToken)
        {
            return _extracurricularScoreComponentHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ExtracurricularScoreComponentEndPoint.GetExtracurricularScoreComponent))]
        [OpenApiOperation(tags: _tag, Summary = "Get Extracurricular Score Component")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetExtracurricularScoreComponentRequest.IdAcademicYear), In = ParameterLocation.Query)]    
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetExtracurricularScoreComponentResult>))]
        public Task<IActionResult> GetExtracurricularScoreComponent([HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _extracurricularScoreComponentHandler.Execute(req, cancellationToken);
        }


        [FunctionName(nameof(ExtracurricularScoreComponentEndPoint.AddExtracurricularScoreComponent))]
        [OpenApiOperation(tags: _tag, Summary = "Add Extracurricular Score Component")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddExtracurricularScoreComponentRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> AddExtracurricularScoreComponent(
         [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route)] HttpRequest req,
         CancellationToken cancellationToken)
        {
            return _extracurricularScoreComponentHandler.Execute(req, cancellationToken);
        }


        [FunctionName(nameof(ExtracurricularScoreComponentEndPoint.DeleteExtracurricularScoreComponent2))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Extracurricular Score Component with Category v2")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(IEnumerable<string>), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> DeleteExtracurricularScoreComponent2(
        [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route+"-v2")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _extracurricularScoreComponentHandler2.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ExtracurricularScoreComponentEndPoint.GetExtracurricularScoreComponent2))]
        [OpenApiOperation(tags: _tag, Summary = "Get Extracurricular Score Component with Category v2")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetExtracurricularScoreComponentRequest2.IdAcademicYear), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetExtracurricularScoreComponentResult2>))]
        public Task<IActionResult> GetExtracurricularScoreComponent2([HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-v2")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _extracurricularScoreComponentHandler2.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ExtracurricularScoreComponentEndPoint.AddExtracurricularScoreComponent2))]
        [OpenApiOperation(tags: _tag, Summary = "Add Extracurricular Score Component  with Category v2")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddExtracurricularScoreComponentRequest2))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> AddExtracurricularScoreComponent2(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "-v2")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _extracurricularScoreComponentHandler2.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ExtracurricularScoreComponentEndPoint.GetExtracurricularScoreCalcultionType))]
        [OpenApiOperation(tags: _tag, Summary = "Get Extracurricular Score Calculation Type")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetExtracurricularScoreCalculationTypeRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetExtracurricualrScoreCalculationTypeResult>))]
        public Task<IActionResult> GetExtracurricularScoreCalcultionType([HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-calculation-type")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _getExtracurricularScoreCalculationTypeHandler.Execute(req, cancellationToken);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.ElectiveCoach;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Scheduling.FnExtracurricular.ElectiveCoach
{
    public class ElectiveCoachEndpoint
    {
        private const string _route = "elective/coach";
        private const string _tag = "Elective Coach";

        private readonly GetElectiveCoachHandler _getElectiveCoachHandler;
        private readonly GetElectiveExternalCoachHandler2 _getElectiveExternalCoachHandler;
        private readonly GetElectiveExternalCoachDetailHandler _getElectiveExternalCoachDetailHandler;
        private readonly UpdateElectiveExternalCoachHandler _updateElectiveExternalCoachHandler;
        private readonly GetExtCoachTaxStatusHandler _getExtCoachTaxStatusHandler;
        private readonly DeleteElectiveExternalCoachHandler _deleteElectiveExternalCoachHandler;

        public ElectiveCoachEndpoint(
        GetElectiveCoachHandler getElectiveCoachHandler,
        GetElectiveExternalCoachHandler2 getElectiveExternalCoachHandler,
        GetElectiveExternalCoachDetailHandler getElectiveExternalCoachDetailHandler,
        UpdateElectiveExternalCoachHandler updateElectiveExternalCoachHandler,
        GetExtCoachTaxStatusHandler getExtCoachTaxStatusHandler,
        DeleteElectiveExternalCoachHandler deleteElectiveExternalCoachHandler)
        {
            _getElectiveCoachHandler = getElectiveCoachHandler;
            _getElectiveExternalCoachHandler = getElectiveExternalCoachHandler;
            _getElectiveExternalCoachDetailHandler = getElectiveExternalCoachDetailHandler;
            _updateElectiveExternalCoachHandler = updateElectiveExternalCoachHandler;
            _getExtCoachTaxStatusHandler = getExtCoachTaxStatusHandler;
            _deleteElectiveExternalCoachHandler = deleteElectiveExternalCoachHandler;
        }

        [FunctionName(nameof(ElectiveCoachEndpoint.GetElectiveCoach))]
        [OpenApiOperation(tags: _tag, Summary = "Get All Staff for Electives Coach")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetElectiveCoachRequest.IdSchool), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetElectiveCoachResult>))]
        public Task<IActionResult> GetElectiveCoach([HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _getElectiveCoachHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ElectiveCoachEndpoint.GetElectiveExternalCoach))]
        [OpenApiOperation(tags: _tag, Summary = "Get Electives External Coach List")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(CollectionRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]    
        [OpenApiParameter(nameof(CollectionRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetElectiveExternalCoachRequest.IdSchool), In = ParameterLocation.Query, Required = true)]      
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetElectiveExternalCoachResult>))]
        public Task<IActionResult> GetElectiveExternalCoach(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "elective/ext-coach/user")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _getElectiveExternalCoachHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ElectiveCoachEndpoint.GetElectiveExternalCoachDetail))]
        [OpenApiOperation(tags: _tag, Summary = "Get Elective External Coach Detail")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetElectiveExternalCoachDetailRequest.IdExtracurricularExternalCoach), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetElectiveExternalCoachDetailResult))]
        public Task<IActionResult> GetElectiveExternalCoachDetail(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "elective/ext-coach/user-detail")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _getElectiveExternalCoachDetailHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ElectiveCoachEndpoint.UpdateElectiveExternalCoach))]
        [OpenApiOperation(tags: _tag, Summary = "Update Elective External Coach")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateElectiveExternalCoachRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateElectiveExternalCoach(
        [HttpTrigger(AuthorizationLevel.Function, "put", Route = "elective/ext-coach/update-user")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _updateElectiveExternalCoachHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ElectiveCoachEndpoint.GetExtCoachTaxStatus))]
        [OpenApiOperation(tags: _tag, Summary = "Get External Coach Tax Status")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]      
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetExtCoachTaxStatusResult>))]
        public Task<IActionResult> GetExtCoachTaxStatus(
       [HttpTrigger(AuthorizationLevel.Function, "get", Route = "elective/ext-coach/taxstatus")] HttpRequest req,
       CancellationToken cancellationToken)
        {
            return _getExtCoachTaxStatusHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ElectiveCoachEndpoint.DeleteElectiveExternalCoach))]
        [OpenApiOperation(tags: _tag, Summary = "Deete Elective External Coach")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(DeleteElectiveExternalCoachRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> DeleteElectiveExternalCoach(
        [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "elective/ext-coach/delete-user")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _deleteElectiveExternalCoachHandler.Execute(req, cancellationToken);
        }
    }
}

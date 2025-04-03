using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.School.FnSchool.VenueMapping;
using BinusSchool.Data.Model.School.FnSchool.VenueType;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace BinusSchool.School.FnSchool.VenueMapping
{
    public class VenueMappingEndpoint
    {
        private const string _route = "school/venue-mapping";
        private const string _tag = "School Venue Mapping";

        private readonly GetVenueMappingHandler _getVenueMappingHandler;
        private readonly SaveVenueMappingHandler _saveVenueMappingHandler;
        private readonly CopyVenueMappingHandler _copyVenueMappingHandler;
        private readonly ExportToExcelVenueMappingHandler _exportToExcelVenueMappingHandler;

        public VenueMappingEndpoint(
            GetVenueMappingHandler getVenueMappingHandler,
            SaveVenueMappingHandler saveVenueMappingHandler,
            CopyVenueMappingHandler copyVenueMappingHandler,
            ExportToExcelVenueMappingHandler exportToExcelVenueMappingHandler)
        {
            _getVenueMappingHandler = getVenueMappingHandler;
            _saveVenueMappingHandler = saveVenueMappingHandler;
            _copyVenueMappingHandler = copyVenueMappingHandler;
            _exportToExcelVenueMappingHandler = exportToExcelVenueMappingHandler;
        }

        [FunctionName(nameof(GetVenueMapping))]
        [OpenApiOperation(tags: _tag, Summary = "Get Venue Mapping")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetVenueMappingRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetVenueMappingRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetVenueMappingRequest.IdBuilding), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetVenueMappingResult>))]
        public Task<IActionResult> GetVenueMapping([HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-venue-mapping")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _getVenueMappingHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(SaveVenueMapping))]
        [OpenApiOperation(tags: _tag, Summary = "Save Venue Mapping")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(SaveVenueMappingRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> SaveVenueMapping([HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/save-venue-mapping")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _saveVenueMappingHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(CopyVenueMapping))]
        [OpenApiOperation(tags: _tag, Summary = "Copy Venue Mapping")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(CopyVenueMappingRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> CopyVenueMapping([HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/copy-venue-mapping")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _copyVenueMappingHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ExportToExcelVenueMapping))]
        [OpenApiOperation(tags: _tag, Summary = "Export To Excel Venue Mapping")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(ExportToExcelVenueMappingRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> ExportToExcelVenueMapping([HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/export-to-excel-venue-mapping")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _exportToExcelVenueMappingHandler.Execute(req, cancellationToken);
        }
    }
}

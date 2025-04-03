using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.School.FnSchool.MasterEquipment.EquipmentType;
using BinusSchool.School.FnSchool.MasterEquipment.EquipmentType;
using BinusSchool.Data.Model.School.FnSchool.MasterEquipment.EquipmentDetails;
using BinusSchool.School.FnSchool.MasterEquipment.EquipmentDetails;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace BinusSchool.School.FnSchool.MasterEquipment
{
    public class MasterEquipmentEndpoint
    {
        private const string _route = "school/master-equipment";
        private const string _tag = "School Master Equipment";

        private readonly DeleteEquipmentTypeHandler _deleteEquipmentTypeHandler;
        private readonly SaveEquipmentTypeHandler _saveEquipmentTypeHandler;
        private readonly GetListEquipmentTypeHandler _getListEquipmentTypeHandler;
        private readonly GetDetailEquipmentTypeHandler _getDetailEquipmentTypeHandler;
        private readonly GetDDLEquipmentTypeHandler _getDDLEquipmentTypeHandler;

        private readonly GetListEquipmentDetailsHandler _getListEquipmentDetailsHandler;
        private readonly GetDetailEquipmentDetailsHandler _getDetailEquipmentDetailsHandler;
        private readonly SaveEquipmentDetailsHandler _saveEquipmentDetailsHandler;
        private readonly DeleteEquipmentDetailsHandler _deleteEquipmentDetailsHandler;

        public MasterEquipmentEndpoint(
            DeleteEquipmentTypeHandler deleteEquipmentTypeHandler,
            SaveEquipmentTypeHandler saveEquipmentTypeHandler,
            GetListEquipmentTypeHandler getListEquipmentTypeHandler,
            GetDetailEquipmentTypeHandler getDetailEquipmentTypeHandler,
            GetDDLEquipmentTypeHandler getDDLEquipmentTypeHandler,
            GetListEquipmentDetailsHandler getListEquipmentDetailsHandler,
            GetDetailEquipmentDetailsHandler getDetailEquipmentDetailsHandler,
            SaveEquipmentDetailsHandler saveEquipmentDetailsHandler,
            DeleteEquipmentDetailsHandler deleteEquipmentDetailsHandler)
        {
            _deleteEquipmentTypeHandler = deleteEquipmentTypeHandler;
            _saveEquipmentTypeHandler = saveEquipmentTypeHandler;
            _getListEquipmentTypeHandler = getListEquipmentTypeHandler;
            _getDetailEquipmentTypeHandler = getDetailEquipmentTypeHandler;
            _getDDLEquipmentTypeHandler = getDDLEquipmentTypeHandler;
            _getListEquipmentDetailsHandler = getListEquipmentDetailsHandler;
            _getDetailEquipmentDetailsHandler = getDetailEquipmentDetailsHandler;
            _saveEquipmentDetailsHandler = saveEquipmentDetailsHandler;
            _deleteEquipmentDetailsHandler = deleteEquipmentDetailsHandler;
        }

        [FunctionName(nameof(DeleteEquipmentType))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Equipment Type")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(DeleteEquipmentTypeRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> DeleteEquipmentType(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/delete-equipment-type")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _deleteEquipmentTypeHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(SaveEquipmentType))]
        [OpenApiOperation(tags: _tag, Summary = "Save Equipment Type")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(SaveEquipmentTypeRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> SaveEquipmentType(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/save-equipment-type")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _saveEquipmentTypeHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(GetListEquipmentType))]
        [OpenApiOperation(tags: _tag, Summary = "Get List Equipment Type")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetListEquipmentTypeRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetListEquipmentTypeResult>))]
        public Task<IActionResult> GetListEquipmentType(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-list-equipment-type")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _getListEquipmentTypeHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(GetDetailEquipmentType))]
        [OpenApiOperation(tags: _tag, Summary = "Get Detail Equipment Type")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetDetailEquipmentTypeRequest.IdEquipmentType), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetDetailEquipmentTypeResult))]
        public Task<IActionResult> GetDetailEquipmentType(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-detail-equipment-type")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _getDetailEquipmentTypeHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(GetDDLEquipmentType))]
        [OpenApiOperation(tags: _tag, Summary = "Get DDL Equipment Type")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetDDLEquipmentTypeRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<NameValueVm>))]
        public Task<IActionResult> GetDDLEquipmentType(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-ddl-equipment-type")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _getDDLEquipmentTypeHandler.Execute(req, cancellationToken);
        }


        [FunctionName(nameof(GetListEquipmentDetails))]
        [OpenApiOperation(tags: _tag, Summary = "Get List Equipment Details")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetListEquipmentDetailsRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetListEquipmentDetailsRequest.IdEquipmentType), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetListEquipmentDetailsResult>))]
        public Task<IActionResult> GetListEquipmentDetails(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-list-equipment-details")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _getListEquipmentDetailsHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(GetDetailEquipmentDetails))]
        [OpenApiOperation(tags: _tag, Summary = "Get Detail Equipment Details")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetDetailEquipmentDetailsRequest.IdEquipment), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetDetailEquipmentDetailsResult))]
        public Task<IActionResult> GetDetailEquipmentDetails(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-detail-equipment-details")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _getDetailEquipmentDetailsHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(SaveEquipmentDetails))]
        [OpenApiOperation(tags: _tag, Summary = "Save Equipment Details")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(SaveEquipmentDetailsRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> SaveEquipmentDetails(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/save-equipment-details")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _saveEquipmentDetailsHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(DeleteEquipmentDetails))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Equipment Details")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(DeleteEquipmentDetailsRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> DeleteEquipmentDetails(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/delete-equipment-details")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _deleteEquipmentDetailsHandler.Execute(req, cancellationToken);
        }
    }
}

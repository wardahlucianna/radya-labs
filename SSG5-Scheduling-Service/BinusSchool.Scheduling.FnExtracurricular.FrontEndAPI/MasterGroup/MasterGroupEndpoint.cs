using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.MasterGroup;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Scheduling.FnExtracurricular.MasterGroup
{
    public class MasterGroupEndpoint
    {
        private const string _route = "master-group";
        private const string _tag = "Extracurricular Master Group";

        private readonly GetMasterGroupHandler _getMasterGroupHandler;
        private readonly CreateMasterGroupHandler _createMasterGroupHandler;
        private readonly UpdateMasterGroupHandler _updateMasterGroupHandler;
        private readonly DeleteMasterGroupHandler _deleteMasterGroupHandler;
        private readonly ExportExcelMasterGroupHandler _exportExcelMasterGroupHandler;

        public MasterGroupEndpoint(
            GetMasterGroupHandler getMasterGroupHandler, 
            CreateMasterGroupHandler createMasterGroupHandler,
            UpdateMasterGroupHandler updateMasterGroupHandler,
            DeleteMasterGroupHandler deleteMasterGroupHandler,
            ExportExcelMasterGroupHandler exportExcelMasterGroupHandler)
        {
            _getMasterGroupHandler = getMasterGroupHandler;
            _createMasterGroupHandler = createMasterGroupHandler;
            _updateMasterGroupHandler = updateMasterGroupHandler;
            _deleteMasterGroupHandler = deleteMasterGroupHandler;
            _exportExcelMasterGroupHandler = exportExcelMasterGroupHandler;
        }

        [FunctionName(nameof(MasterGroupEndpoint.GetMasterGroupHandler))]
        [OpenApiOperation(tags: _tag, Summary = "Get Extracurricular Master Group")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(GetMasterGroupRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetMasterGroupResult))]
        public Task<IActionResult> GetMasterGroupHandler([HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/get-master-group")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _getMasterGroupHandler.Execute(req, cancellationToken, false);
        }

        [FunctionName(nameof(MasterGroupEndpoint.CreateMasterGroup))]
        [OpenApiOperation(tags: _tag, Summary = "Create Extracurricular Master Group")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(CreateMasterGroupRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> CreateMasterGroup([HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/create-master-group")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _createMasterGroupHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MasterGroupEndpoint.UpdateMasterGroup))]
        [OpenApiOperation(tags: _tag, Summary = "Update Extracurricular Master Group")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateMasterGroupRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateMasterGroup([HttpTrigger(AuthorizationLevel.Function, "put", Route = _route + "/update-master-group")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _updateMasterGroupHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MasterGroupEndpoint.DeleteMasterGroup))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Extracurricular Master Group")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(DeleteMasterGroupRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> DeleteMasterGroup([HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route + "/delete-master-group")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _deleteMasterGroupHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MasterGroupEndpoint.ExportExcelMasterGroup))]
        [OpenApiOperation(tags: _tag, Summary = "Export Extracurricular Master Group to Excel")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(ExportExcelMasterGroupRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> ExportExcelMasterGroup([HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/master-group-excel")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _exportExcelMasterGroupHandler.Execute(req, cancellationToken, false);
        }
    }
}

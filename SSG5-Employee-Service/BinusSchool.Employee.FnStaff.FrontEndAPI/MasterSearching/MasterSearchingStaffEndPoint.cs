using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Data.Model.Employee.MasterSearching;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Employee.FnStaff.MasterSearching
{
    public class MasterSearchingStaffEndPoint
    {

        private const string _route = "staff/MasterSearching";
        private const string _tag = "MasterSearching";

        [FunctionName(nameof(MasterSearchingStaffEndPoint.GetMasterSearchingData))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(GetMasterSearchingDataforStaffRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> GetMasterSearchingData(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/GetMasterSearchingData")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetMasterSearchingDataforStaffHandler>();
            return handler.Execute(req, cancellationToken, false);
        }

        [FunctionName(nameof(MasterSearchingStaffEndPoint.GetFieldDataList))]
        [OpenApiOperation(tags: _tag, Summary = "Get Field Data List")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetFieldDataListforMasterSearchingStaffResult))]
        public Task<IActionResult> GetFieldDataList(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/GetFieldDataList")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetStaffFieldDataListHandler>();
            return handler.Execute(req, cancellationToken, false);
        }

        [FunctionName(nameof(MasterSearchingStaffEndPoint.ExportMasterSearchingResultToExcel))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(ExportToExcelMasterSearchingStaffDataRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> ExportMasterSearchingResultToExcel(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "-export")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<ExportMasterSearchingStaffResultToExcelHandler>();
            return handler.Execute(req, cancellationToken, false);
        }

    }
}

using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Data.Model.Document.FnDocument.BLPGroup;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Document.FnDocument.BLPGroup
{
    public class BLPGroupEndpoint
    {
        private const string _route = "blp/blp-group";
        private const string _tag = "Blended Learning Program Group";

        private readonly BLPGroupHandler _bLPGroupHandler;
        private readonly BLPGroupStudentHandler _bLPGroupStudentHandler;
        private readonly UpdateBLPGroupStudentHandler _updateBLPGroupStudentHandler;
        private readonly ExportExcelBLPGroupStudentHandler _exportExcelBLPGroupStudentHandler;

        public BLPGroupEndpoint(BLPGroupHandler bLPGroupHandler,
            BLPGroupStudentHandler bLPGroupStudentHandler,
            UpdateBLPGroupStudentHandler updateBLPGroupStudentHandler,
            ExportExcelBLPGroupStudentHandler exportExcelBLPGroupStudentHandler
            )
        {
            _bLPGroupHandler = bLPGroupHandler;
            _bLPGroupStudentHandler = bLPGroupStudentHandler;
            _updateBLPGroupStudentHandler = updateBLPGroupStudentHandler;
            _exportExcelBLPGroupStudentHandler = exportExcelBLPGroupStudentHandler;
        }

        [FunctionName(nameof(BLPGroupEndpoint.GetBLPGroup))]
        [OpenApiOperation(tags: _tag, Summary = "Get BLP Group")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetBLPGroupRequest.IdLevel), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetBLPGroupResult>))]
        public Task<IActionResult> GetBLPGroup([HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _bLPGroupHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(BLPGroupEndpoint.GetBLPGroupDetail))]
        [OpenApiOperation(tags: _tag, Summary = "Get BLP Group Detail")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter("id", Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetBLPGroupDetailResult))]
        public Task<IActionResult> GetBLPGroupDetail(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/{id}")] HttpRequest req,
        string id,
        CancellationToken cancellationToken)
        {
            return _bLPGroupHandler.Execute(req, cancellationToken, true, "id".WithValue(id));
        }

        [FunctionName(nameof(BLPGroupEndpoint.SaveBLPGroup))]
        [OpenApiOperation(tags: _tag, Summary = "Save BLP Group")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(SaveBLPGroupRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> SaveBLPGroup([HttpTrigger(AuthorizationLevel.Function, "post", Route = _route)] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _bLPGroupHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(BLPGroupEndpoint.UpdateBLPGroup))]
        [OpenApiOperation(tags: _tag, Summary = "Update BLP Group")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateBLPGroupRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateBLPGroup([HttpTrigger(AuthorizationLevel.Function, "put", Route = _route)] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _bLPGroupHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(BLPGroupEndpoint.DeleteBLPGroup))]
        [OpenApiOperation(tags: _tag, Summary = "Delete BLP Group")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(IEnumerable<string>), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> DeleteBLPGroup(
        [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route)] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _bLPGroupHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(BLPGroupEndpoint.GetBLPGroupStudent))]
        [OpenApiOperation(tags: _tag, Summary = "Get BLP Group Student")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetBLPGroupStudentRequest.IdAcademicYear), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetBLPGroupStudentRequest.IdLevel), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetBLPGroupStudentRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetBLPGroupStudentRequest.Semester), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetBLPGroupStudentRequest.BLPFinalStatus), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetBLPGroupStudentResult>))]
        public Task<IActionResult> GetBLPGroupStudent([HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-student")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _bLPGroupStudentHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(BLPGroupEndpoint.UpdateBLPGroupStudent))]
        [OpenApiOperation(tags: _tag, Summary = "Insert/Update BLP Group Student")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(List<UpdateBLPGroupStudentRequest>))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateBLPGroupStudent([HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "-student")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _updateBLPGroupStudentHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(BLPGroupEndpoint.ExportExcelBLPGroupStudent))]
        [OpenApiOperation(tags: _tag, Summary = "Export BLP Group Student to Excel")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(ExportExcelBLPGroupStudentRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> ExportExcelBLPGroupStudent([HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "-student-excel")] HttpRequest req,
       CancellationToken cancellationToken)
        {
            return _exportExcelBLPGroupStudentHandler.Execute(req, cancellationToken, false);
        }
    }
}

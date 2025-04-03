using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Data.Model.Student.FnStudent.MasterSearching;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Student.FnStudent.MasterSearching
{
    public class MasterSearchingEndPoint
    {
        private const string _route = "student/MasterSearching";
        private const string _tag = "MasterSearching";

        [FunctionName(nameof(MasterSearchingEndPoint.GetFieldDataList))]
        [OpenApiOperation(tags: _tag, Summary = "Get Field Data List")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetFieldDataListRequest.IdUser), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetFieldDataListRequest.IdDataFieldRole), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetFieldDataListResult))]
        public Task<IActionResult> GetFieldDataList(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/GetFieldDataList")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetFieldDataListHandler>();
            return handler.Execute(req, cancellationToken, false);
        }

        [FunctionName(nameof(MasterSearchingEndPoint.ExportMasterSearchingResultToExcel))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(ExportToExcelMasterSearchingDataRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> ExportMasterSearchingResultToExcel(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "-export")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<ExportMasterSearchingResultToExcelHandler>();
            return handler.Execute(req, cancellationToken, false);
        }

        [FunctionName(nameof(MasterSearchingEndPoint.ExportMasterSeachingDataTableToExcel))]
        [OpenApiOperation(tags: _tag, Summary = "Export Master Seaching Data Table To Excel")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(ExportMasterSeachingDataTableToExcelRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> ExportMasterSeachingDataTableToExcel(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "-exportexcel")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<ExportMasterSeachingDataTableToExcelHandler>();
            return handler.Execute(req, cancellationToken, false);
        }

        [FunctionName(nameof(MasterSearchingEndPoint.GetMasterSearchingData))]
        [OpenApiOperation(tags: _tag, Summary = "Get Master Searching Data")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(GetMasterSearchingDataRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> GetMasterSearchingData(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/GetMasterSearchingData")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetMasterSearchingDataHandler>();
            return handler.Execute(req, cancellationToken, false);
        }

        [FunctionName(nameof(MasterSearchingEndPoint.GetMasterSearchingDataTable))]
        [OpenApiOperation(tags: _tag, Summary = "Get Master Searching Data Table")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(GetMasterSearchingDataTableRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetMasterSearchingDataTableResult))]
        public Task<IActionResult> GetMasterSearchingDataTable(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/GetMasterSearchingDataTable")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetMasterSearchingDataTableHandler>();
            return handler.Execute(req, cancellationToken, false);
        }

        [FunctionName(nameof(MasterSearchingEndPoint.GetActiveAYandSmt))]
        [OpenApiOperation(tags: _tag, Summary = "Get Active AY and Smt")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetActiveAYandSmtRequest.IdSchool), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(IEnumerable<GetActiveAYandSmtResult>))]
        public Task<IActionResult> GetActiveAYandSmt(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/GetActiveAYandSmt")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetActiveAYandSmtHandler>();
            return handler.Execute(req, cancellationToken, false);
        }

        [FunctionName(nameof(MasterSearchingEndPoint.GetListHomeroomByAySmtLvlGrd))]
        [OpenApiOperation(tags: _tag, Summary = "Get List Homeroom By Ay Smt Lvl Grd")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetListHomeroomByAySmtLvlGrdRequest.IdAcademicYear), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListHomeroomByAySmtLvlGrdRequest.Semester), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListHomeroomByAySmtLvlGrdRequest.IdLevel), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListHomeroomByAySmtLvlGrdRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListHomeroomByAySmtLvlGrdRequest.Search), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(IEnumerable<GetListHomeroomByAySmtLvlGrdResult>))]
        public Task<IActionResult> GetListHomeroomByAySmtLvlGrd(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/GetListHomeroom")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetListHomeroomByAySmtLvlGrdHandler>();
            return handler.Execute(req, cancellationToken, false);
        }

        [FunctionName(nameof(MasterSearchingEndPoint.GetListStudentByAySmtLvlGrdHrm))]
        [OpenApiOperation(tags: _tag, Summary = "Get List Student By Ay Smt Lvl Grd Hrm")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetListStudentByAySmtLvlGrdHrmRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListStudentByAySmtLvlGrdHrmRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetListStudentByAySmtLvlGrdHrmRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetListStudentByAySmtLvlGrdHrmRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListStudentByAySmtLvlGrdHrmRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListStudentByAySmtLvlGrdHrmRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetListStudentByAySmtLvlGrdHrmRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListStudentByAySmtLvlGrdHrmRequest.IdAcademicYear), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListStudentByAySmtLvlGrdHrmRequest.Semester), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListStudentByAySmtLvlGrdHrmRequest.IdLevel), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListStudentByAySmtLvlGrdHrmRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListStudentByAySmtLvlGrdHrmRequest.IdHomeroom), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetListStudentByAySmtLvlGrdHrmResult))]
        public Task<IActionResult> GetListStudentByAySmtLvlGrdHrm(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/GetListStudent")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetListStudentByAySmtLvlGrdHrmHandler>();
            return handler.Execute(req, cancellationToken, false);
        }
    }
}

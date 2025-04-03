using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.MasterExtracurricular;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Scheduling.FnExtracurricular.MasterExtracurricular
{
    public class MasterExtracurricularEndPoint
    {
        private const string _route = "master-extracurricular";
        private const string _tag = "Master Extracurricular";

        [FunctionName(nameof(MasterExtracurricularEndPoint.GetMasterExtracurriculars))]
        [OpenApiOperation(tags: _tag, Summary = "Get Master Extracurricular List")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetMasterExtracurricularRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetMasterExtracurricularRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetMasterExtracurricularRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetMasterExtracurricularRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetMasterExtracurricularRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetMasterExtracurricularRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetMasterExtracurricularRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetMasterExtracurricularRequest.IdAcademicYear), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetMasterExtracurricularRequest.IdLevel), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetMasterExtracurricularRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetMasterExtracurricularRequest.Semester), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetMasterExtracurricularRequest.Status), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetMasterExtracurricularRequest.IdUser), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetMasterExtracurricularRequest.IdElectiveGroup), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetMasterExtracurricularRequest.ScheduleDay), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetMasterExtracurricularResult))]
        public Task<IActionResult> GetMasterExtracurriculars(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-master-extracurricular")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetMasterExtracurricularHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MasterExtracurricularEndPoint.GetMasterExtracurricularDetail))]
        [OpenApiOperation(tags: _tag, Summary = "Get Master Extracurricular Detail")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetMasterExtracurricularDetailRequest.Id), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetMasterExtracurricularDetailResult))]
        public Task<IActionResult> GetMasterExtracurricularDetail(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/getdetail-master-extracurricular")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetMasterExtracurricularDetailHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MasterExtracurricularEndPoint.AddMasterExtracurricular))]
        [OpenApiOperation(tags: _tag, Summary = "Add Master Extracurricular")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddMasterExtracurricularRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> AddMasterExtracurricular(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/add-master-extracurricular")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<AddMasterExtracurricularHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MasterExtracurricularEndPoint.UpdateMasterExtracurricular))]
        [OpenApiOperation(tags: _tag, Summary = "Update Master Extracurricular")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateMasterExtracurricularRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateMasterExtracurricular(
           [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route + "/update-master-extracurricular")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<UpdateMasterExtracurricularHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MasterExtracurricularEndPoint.DeleteMasterExtracurricular))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Master Extracurricular")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(DeleteMasterExtracurricularRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> DeleteMasterExtracurricular(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route + "/delete-master-extracurricular")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<DeleteMasterExtracurricularHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MasterExtracurricularEndPoint.TransferMasterExtracurricular))]
        [OpenApiOperation(tags: _tag, Summary = "Transfer Master Extracurricular")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(TransferMasterExtracurricularRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> TransferMasterExtracurricular(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/transfer-master-extracurricular")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<TransferMasterExtracurricularHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MasterExtracurricularEndPoint.ExportExcelMasterExtracurricular))]
        [OpenApiOperation(tags: _tag, Summary = "Export Excel Master Extracurricular")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(ExportExcelMasterExtracurricularRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> ExportExcelMasterExtracurricular([HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/export-excel-master-extracurricular")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<ExportExcelMasterExtracurricularHandler>();
            return handler.Execute(req, cancellationToken, false);
        }


        [FunctionName(nameof(MasterExtracurricularEndPoint.UpdateSESnECUserRole))]
        [OpenApiOperation(tags: _tag, Summary = "Update SES n EC UserRole")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateSESnECUserRoleRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateSESnECUserRole(
           [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route + "/update-SES-EC-UserRole")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<UpdateSESnECUserRoleHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MasterExtracurricularEndPoint.GetElectivesByLevel))]
        [OpenApiOperation(tags: _tag, Summary = "Get Electives ByLevel")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetElectivesByLevelRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetElectivesByLevelRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetElectivesByLevelRequest.IdLevel), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetElectivesByLevelResult))]
        public Task<IActionResult> GetElectivesByLevel([HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-electives-byLevel")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetElectivesByLevelHandler>();
            return handler.Execute(req, cancellationToken, false);
        }


        [FunctionName(nameof(MasterExtracurricularEndPoint.UpdateElectivesEntryPeriod))]
        [OpenApiOperation(tags: _tag, Summary = "Update Electives Entry Period")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateElectivesEntryPeriodRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateElectivesEntryPeriod(
           [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/update-electives-entry-period")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<UpdateElectivesEntryPeriodHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MasterExtracurricularEndPoint.GetMasterExtracurricularsV2))]
        [OpenApiOperation(tags: _tag, Summary = "Get Master Extracurricular List V2")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(GetMasterExtracurricularV2Request))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetMasterExtracurricularResult))]
        public Task<IActionResult> GetMasterExtracurricularsV2(
           [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/get-master-extracurricularV2")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetMasterExtracurricularV2Handler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MasterExtracurricularEndPoint.GetMasterExtracurricularType))]
        [OpenApiOperation(tags: _tag, Summary = "Get Master Extracurricular Type")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetMasterExtracurricularTypeRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetMasterExtracurricularTypeRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetMasterExtracurricularTypeRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetMasterExtracurricularTypeRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetMasterExtracurricularTypeRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetMasterExtracurricularTypeRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetMasterExtracurricularTypeRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetMasterExtracurricularTypeRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetMasterExtracurricularTypeResult))]
        public Task<IActionResult> GetMasterExtracurricularType(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-master-extracurricular-type")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetMasterExtracurricularTypeHandler>();
            return handler.Execute(req, cancellationToken);
        }
    }
}

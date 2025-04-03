using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnGuidanceCounseling.CounselingServiceEntry;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Student.FnGuidanceCounseling.CounselingServiceEntry
{
    public class CounselingServiceEntryEndpoint
    {
        private const string _route = "guidance-counseling/counseling-service-entry";
        private const string _tag = "Counseling Service Entry";

        [FunctionName(nameof(CounselingServiceEntryEndpoint.GetListCounselingServiceEntry))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetCounselingServiceEntryRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetCounselingServiceEntryRequest.IdLevel), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetCounselingServiceEntryRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetCounselingServiceEntryRequest.IdHomeRoom), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetCounselingServiceEntryRequest.IdCounselingCategory), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetCounselingServiceEntryRequest.Semester), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetCounselingServiceEntryRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetCounselingServiceEntryRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetCounselingServiceEntryRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetCounselingServiceEntryRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetCounselingServiceEntryRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetCounselingServiceEntryRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetCounselingServiceEntryRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetCounselingServiceEntryResult))]
        public Task<IActionResult> GetListCounselingServiceEntry(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<CounselingServiceEntryHandler>();
            return handler.Execute(req, cancellationToken, false);
        }

        [FunctionName(nameof(CounselingServiceEntryEndpoint.GetCounselingServiceEntryDetail))]
        [OpenApiOperation(tags: _tag, Summary = "Get Counseling Service Entry Detail")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter("id", Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetCounselingServiceEntryDetailResult))]
        public Task<IActionResult> GetCounselingServiceEntryDetail(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/detail/{id}")] HttpRequest req,
            string id,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<CounselingServiceEntryHandler>();
            return handler.Execute(req, cancellationToken, keyValues: "id".WithValue(id));
        }

        [FunctionName(nameof(CounselingServiceEntryEndpoint.AddCounselingServiceEntry))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddCounselingServiceEntryRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> AddCounselingServiceEntry(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<CounselingServiceEntryHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(CounselingServiceEntryEndpoint.UpdateCounselingServiceEntry))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateCounselingServiceEntryRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateCounselingServiceEntry(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<CounselingServiceEntryHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(CounselingServiceEntryEndpoint.DeleteCouselingServiceEntry))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Counseling Service Entry")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(IEnumerable<string>), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> DeleteCouselingServiceEntry(
        [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route)] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<CounselingServiceEntryHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(CounselingServiceEntryEndpoint.GetCounselingServiceEntryStudentHistory))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetCounselingServiceEntryStudentHistoryRequest.IdStudent), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetCounselingServiceEntryStudentHistoryRequest.IdAcademicYear), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetCounselingServiceEntryRequest.IdCounselingCategory), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetCounselingServiceEntryRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetCounselingServiceEntryRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetCounselingServiceEntryRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetCounselingServiceEntryRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetCounselingServiceEntryRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetCounselingServiceEntryRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetCounselingServiceEntryRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetCounselingServiceEntryStudentHistoryResult))]
        public Task<IActionResult> GetCounselingServiceEntryStudentHistory(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/history")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetCounselingServiceEntryStudentHistoryHandler>();
            return handler.Execute(req, cancellationToken, false);
        }

        [FunctionName(nameof(CounselingServiceEntryEndpoint.GetDownloadSummaryCounselingServiceEntry))]
        [OpenApiOperation(tags: _tag, Summary = "Download summary counseling service entry")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(DownloadSummaryCounselingServiceEntryRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> GetDownloadSummaryCounselingServiceEntry(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/download-excel")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<DownloadSummaryCounselingServiceEntryHandler>();
            return handler.Execute(req, cancellationToken, false);
        }

        [FunctionName(nameof(CounselingServiceEntryEndpoint.GetCounselorGrade))]
        [OpenApiOperation(tags: _tag, Summary = "Get Counselor Grade")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetCounselorGradeRequest.IdUser), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetCounselorGradeResult[]))]
        public Task<IActionResult> GetCounselorGrade(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/list-grade")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetCounselorGradeHandler>();
            return handler.Execute(req, cancellationToken, false);
        }

        [FunctionName(nameof(CounselingServiceEntryEndpoint.GetCounselorLevel))]
        [OpenApiOperation(tags: _tag, Summary = "Get Counselor Level")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetCounselorLevelRequest.IdUser), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetCounselorLevelResult[]))]
        public Task<IActionResult> GetCounselorLevel(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/list-level")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetCounselorLevelHandler>();
            return handler.Execute(req, cancellationToken, false);
        }

        [FunctionName(nameof(CounselingServiceEntryEndpoint.GetCounselorHomeRoom))]
        [OpenApiOperation(tags: _tag, Summary = "Get Counselor Home Room")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetCounselorHomeRoomRequest.IdUser), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetCounselorHomeRoomResult[]))]
        public Task<IActionResult> GetCounselorHomeRoom(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/list-home-room")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetCounselorHomeRoomHandler>();
            return handler.Execute(req, cancellationToken, false);
        }

        [FunctionName(nameof(CounselingServiceEntryEndpoint.GetCounselorLevelByCounsellor))]
        [OpenApiOperation(tags: _tag, Summary = "Get level by counsellor")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetCounselorLevelByCounsellorRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetCounselorLevelByCounsellorRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(CodeWithIdVm[]))]
        public Task<IActionResult> GetCounselorLevelByCounsellor(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/Level-By-Counsellor")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetCounselorLevelByCounsellorHandler>();
            return handler.Execute(req, cancellationToken, false);
        }

        [FunctionName(nameof(CounselingServiceEntryEndpoint.GetCounselorGradeByCounsellor))]
        [OpenApiOperation(tags: _tag, Summary = "Get Grade By Counselor")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetCounselorGradeByCounsellorRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetCounselorGradeByCounsellorRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetCounselorGradeByCounsellorRequest.IdLevel), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(CodeWithIdVm[]))]
        public Task<IActionResult> GetCounselorGradeByCounsellor(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/Grade-By-Counsellor")] HttpRequest req,
            CancellationToken cancellationToken)    
        {

            var handler = req.HttpContext.RequestServices.GetService<GetCounselorGradeByCounsellorHandler>();
            return handler.Execute(req, cancellationToken, false);
        }
    }
}

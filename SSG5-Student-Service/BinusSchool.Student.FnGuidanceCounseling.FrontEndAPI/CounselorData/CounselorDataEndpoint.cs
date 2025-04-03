using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnGuidanceCounseling.CounselorData;
using BinusSchool.Student.FnGuidanceCounseling.ReportStudentToGc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Student.FnGuidanceCounseling.CounselorData
{
    public class CounselorDataEndpoint
    {
        private const string _route = "guidance-counseling/counselor-data";
        private const string _tag = "Counselor Data";

        private readonly CounselorDataHandler _counselorDataHandler;
        private readonly GetListCounselorDataHandler _getListCounselorDataHandler;

        private readonly UsedCounselorDataGradeHandler _usedCounselorDataGradeHandler;
        public CounselorDataEndpoint(CounselorDataHandler counselorDataHandler, UsedCounselorDataGradeHandler usedCounselorDataGradeHandler, GetListCounselorDataHandler getListCounselorDataHandler)
        {
            _counselorDataHandler = counselorDataHandler;
            _usedCounselorDataGradeHandler = usedCounselorDataGradeHandler;
            _getListCounselorDataHandler = getListCounselorDataHandler;
        }

        #region Counselor Data
        [FunctionName(nameof(CounselorDataEndpoint.GetCounselorData))]
        [OpenApiOperation(tags: _tag, Summary = "Get Counselor Data")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetCounselorDataRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetCounselorDataRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetCounselorDataRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetCounselorDataRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetCounselorDataRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetCounselorDataRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetCounselorDataRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetCounselorDataRequest.IdAcadyear), In = ParameterLocation.Query,Required =true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetCounselorDataResult[]))]
        public Task<IActionResult> GetCounselorData(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken
            )
        {
            return _getListCounselorDataHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(CounselorDataEndpoint.GetDetailCounselorData))]
        [OpenApiOperation(tags: _tag, Summary = "Get Detail CounselorData")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter("id", Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetDetailCounselorDataResult))]
        public Task<IActionResult> GetDetailCounselorData(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/{id}")] HttpRequest req,
            string id,
            CancellationToken cancellationToken)
        {
            return _counselorDataHandler.Execute(req, cancellationToken, true, "id".WithValue(id));
        }

        [FunctionName(nameof(CounselorDataEndpoint.AddCounselorData))]
        [OpenApiOperation(tags: _tag, Summary = "Add Counselor Data")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddCounselorDataRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> AddCounselorData(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _counselorDataHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(CounselorDataEndpoint.UpdateCounselorData))]
        [OpenApiOperation(tags: _tag, Summary = "Update Counselor Data")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateCounselorDataRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> UpdateCounselorData(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _counselorDataHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(CounselorDataEndpoint.DeleteCounselorData))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Counseling Category")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(IEnumerable<string>), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> DeleteCounselorData(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _counselorDataHandler.Execute(req, cancellationToken);
        }
        #endregion

        [FunctionName(nameof(CounselorDataEndpoint.GetUsedGrade))]
        [OpenApiOperation(tags: _tag, Summary = "Get Used Grade")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter("id", Required = false)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<string>))]
        public Task<IActionResult> GetUsedGrade(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/used-grade/{id}")] HttpRequest req,
            string id,
            CancellationToken cancellationToken)
        {
            return _usedCounselorDataGradeHandler.Execute(req, cancellationToken, true, "id".WithValue(id));
        }

        [FunctionName(nameof(CounselorDataEndpoint.GetLevelGradeByConcellor))]
        [OpenApiOperation(tags: _tag, Summary = "Get Level Grade By councellor")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetLevelGradeByConcellorRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetLevelGradeByConcellorRequest.IdPosition), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetLevelGradeByConcellorRequest.IdRole), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetLevelGradeByConcellorRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetLevelGradeByConcellorRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetLevelGradeByConcellorResult[]))]
        public Task<IActionResult> GetLevelGradeByConcellor(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-level-grade")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetLevelGradeByConcellorHandler>();
            return handler.Execute(req, cancellationToken);
        }
    }
}

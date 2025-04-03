using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Data.Model.Attendance.FnAttendance.Formula;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Attendance.FnAttendance.Formula
{
    public class FormulaEndpoint
    {
        private const string _route = "formula";
        private const string _tag = "Formula";

        [FunctionName(nameof(FormulaEndpoint.GetFormulaDetail))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter("idLevel", In = ParameterLocation.Path, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(FormulaResult))]
        public Task<IActionResult> GetFormulaDetail(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/detail/{idLevel}")] HttpRequest req,
            string idLevel,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetFormulaDetailHandler>();
            return handler.Execute(req, cancellationToken, keyValues: "idLevel".WithValue(idLevel));
        }

        [FunctionName(nameof(FormulaEndpoint.SetFormula))]
        [OpenApiOperation(tags: _tag, Summary = "Set Formula")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(SetFormulaRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> SetFormula(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<SetFormulaHandler>();
            return handler.Execute(req, cancellationToken);
        }
    }
}

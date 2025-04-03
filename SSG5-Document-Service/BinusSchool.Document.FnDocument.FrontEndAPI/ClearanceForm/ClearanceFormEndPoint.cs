using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Data.Model.Document.FnDocument.ClearanceForm;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Document.FnDocument.ClearanceForm
{
    public class ClearanceFormEndPoint
    {
        private const string _route = "clearance-form";
        private const string _tag = "Clearance Form";

        [FunctionName(nameof(ClearanceFormEndPoint.GetClearanceFormPeriod))]
        [OpenApiOperation(tags: _tag, Summary = "Get Clearance Form Period")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetClearanceFormPeriodRequest.Username), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetClearanceFormPeriodRequest.IsParent), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetClearanceFormPeriodResult))]
        public Task<IActionResult> GetClearanceFormPeriod(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-clearanceform-period")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetClearanceFormPeriodHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ClearanceFormEndPoint.GetAllStudentStatusClearanceForm))]
        [OpenApiOperation(tags: _tag, Summary = "Get All Student Status Clearance Form")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetAllStudentStatusClearanceFormRequest.Username), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAllStudentStatusClearanceFormRequest.IsParent), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetAllStudentStatusClearanceFormResult))]
        public Task<IActionResult> GetAllStudentStatusClearanceForm(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-allstudentstatus-clearanceform")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetAllStudentStatusClearanceFormHandler>();
            return handler.Execute(req, cancellationToken);
        }
    }
}

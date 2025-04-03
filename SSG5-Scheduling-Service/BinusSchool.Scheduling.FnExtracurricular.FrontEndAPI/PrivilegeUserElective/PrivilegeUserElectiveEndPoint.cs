using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.DependencyInjection;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.PrivilegeUserElective;
using System.Collections.Generic;

namespace BinusSchool.Scheduling.FnExtracurricular.PrivilegeUserElective
{
    public class PrivilegeUserElectiveEndPoint
    {
        private const string _route = "privilege-elective";
        private const string _tag = "Privilege User Elective";

        [FunctionName(nameof(PrivilegeUserElectiveEndPoint.GetPrivilegeUserElective))]
        [OpenApiOperation(tags: _tag, Summary = "Get Privilege User Elective")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetPrivilegeUserElectiveRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetPrivilegeUserElectiveRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetPrivilegeUserElectiveRequest.IdAcademicYear), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetPrivilegeUserElectiveResult>))]
        public Task<IActionResult> GetPrivilegeUserElective(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/GetPrivilegeUserElective")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetPrivilegeUserElectiveHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(PrivilegeUserElectiveEndPoint.GetPrivilegeShowButtonUserElective))]
        [OpenApiOperation(tags: _tag, Summary = "Get Privilege Show Button User Elective")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetPrivilegeShowButtonUserElectiveRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetPrivilegeShowButtonUserElectiveRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetPrivilegeShowButtonUserElectiveRequest.IdAcademicYear), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetPrivilegeShowButtonUserElectiveResult))]
        public Task<IActionResult> GetPrivilegeShowButtonUserElective(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/GetPrivilegeShowButtonUserElective")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetPrivilegeShowButtonUserElectiveHandler>();
            return handler.Execute(req, cancellationToken);
        }
    }
}

using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.Student.FnStudent.HandbookManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Student.FnStudent.HandbookManagement
{
    public class HandbookManagementEndPoint
    {
        private const string _route = "student/handbook-management";
        private const string _tag = "Handbook Management";

        [FunctionName(nameof(HandbookManagementEndPoint.GetListHandbookManagement))]
        [OpenApiOperation(tags: _tag, Summary = "Get Handbook Management")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetHandbookManagementRequest.IdUser), In = ParameterLocation.Query,Required =true)]
        [OpenApiParameter(nameof(GetHandbookManagementRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetHandbookManagementRequest.IsHandbookForm), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetHandbookManagementRequest.Idlevel), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetHandbookManagementRequest.ViewFor), In = ParameterLocation.Query, Type = typeof(List<HandbookFor>))]
        [OpenApiParameter(nameof(GetHandbookManagementRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetHandbookManagementRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetHandbookManagementRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetHandbookManagementRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetHandbookManagementRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetHandbookManagementRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetHandbookManagementRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetHandbookManagementResult))]
        public Task<IActionResult> GetListHandbookManagement(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<HandbookManagementHandler>();
            return handler.Execute(req, cancellationToken, false);
        }

        [FunctionName(nameof(HandbookManagementEndPoint.AddHandbookManagement))]
        [OpenApiOperation(tags: _tag, Summary = "Add Handbook Management")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddHandbookManagementRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> AddHandbookManagement(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route)] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<HandbookManagementHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(HandbookManagementEndPoint.UpdateHandbookManagement))]
        [OpenApiOperation(tags: _tag, Summary = "Update Handbook Management")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateHandbookManagementRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateHandbookManagement(
        [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route)] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<HandbookManagementHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(HandbookManagementEndPoint.GetListHandbookManagementDetail))]
        [OpenApiOperation(tags: _tag, Summary = "Get Handbook Management Detail")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter("id", Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetHandbookManagementDetailResult))]
        public Task<IActionResult> GetListHandbookManagementDetail(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/detail/{id}")] HttpRequest req,
        string id,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<HandbookManagementHandler>();
            return handler.Execute(req, cancellationToken, keyValues: "id".WithValue(id));
        }

        [FunctionName(nameof(HandbookManagementEndPoint.DeleteHandbookManagement))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Handbook Management")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(IEnumerable<string>), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> DeleteHandbookManagement(
        [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route)] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<HandbookManagementHandler>();
            return handler.Execute(req, cancellationToken);
        }
    }
}

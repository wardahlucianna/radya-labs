using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Data.Model.School.FnSubject.Department;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace BinusSchool.School.FnSubject.Department
{
    public class DepartmentEndpoint
    {
        private const string _route = "school/department";
        private const string _tag = "School Department";

        private readonly DepartmentHandler _handler;
        private readonly CopyDepartmentHandler _copyHandler;

        public DepartmentEndpoint(DepartmentHandler handler, CopyDepartmentHandler copyHandler)
        {
            _handler = handler;
            _copyHandler = copyHandler;
        }

        [FunctionName(nameof(DepartmentEndpoint.GetDepartments))]
        [OpenApiOperation(tags: _tag, Summary = "Get Department List")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetDepartmentRequest.Ids), In = ParameterLocation.Query, Type = typeof(string[]))]
        [OpenApiParameter(nameof(GetDepartmentRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDepartmentRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDepartmentRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetDepartmentRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetDepartmentRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDepartmentRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDepartmentRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetDepartmentRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDepartmentRequest.IdAcadyear), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDepartmentRequest.IdLevel), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetDepartmentResult[]))]
        public Task<IActionResult> GetDepartments(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(DepartmentEndpoint.GetDepartmentDetail))]
        [OpenApiOperation(tags: _tag, Summary = "Get Department Detail")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter("id", Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetDepartmenDetailResult))]
        public Task<IActionResult> GetDepartmentDetail(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/{id}")] HttpRequest req,
           string id,
           CancellationToken cancellationToken)
        {
            return _handler.Execute(req, cancellationToken, true, "id".WithValue(id));
        }

        [FunctionName(nameof(DepartmentEndpoint.AddDepartment))]
        [OpenApiOperation(tags: _tag, Summary = "Add Department")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddDepartmentRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> AddDepartment(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(DepartmentEndpoint.UpdateDepartment))]
        [OpenApiOperation(tags: _tag, Summary = "Update Department")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateDepartmentRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateDepartment(
           [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route)] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(DepartmentEndpoint.DeleteDepartment))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Department")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(IEnumerable<string>), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> DeleteDepartment(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(DepartmentEndpoint.CopyDepartment))]
        [OpenApiOperation(tags: _tag, Summary = "Copy Department")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(CopyDepartmentRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> CopyDepartment(
           [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/copy")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _copyHandler.Execute(req, cancellationToken);
        }
    }
}

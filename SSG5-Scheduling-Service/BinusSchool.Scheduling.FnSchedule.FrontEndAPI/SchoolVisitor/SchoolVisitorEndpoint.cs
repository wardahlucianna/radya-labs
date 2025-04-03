using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolVisitor;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Scheduling.FnSchedule.SchoolVisitor
{
    public class SchoolVisitorEndPoint
    {
        private const string _route = "schedule/school-visitor";
        private const string _tag = "School Visitor";
        private readonly SchoolVisitorHandler _schoolVisitorHandler;
        public SchoolVisitorEndPoint(SchoolVisitorHandler SchoolVisitorHandler)
        {
            _schoolVisitorHandler = SchoolVisitorHandler;
        }

        [FunctionName(nameof(SchoolVisitorEndPoint.GetSchoolVisitor))]
        [OpenApiOperation(tags: _tag, Summary = "Get Visitor School")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(CollectionRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetSchoolVisitorRequest.VisitDate), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSchoolVisitorRequest.VisitorType), In = ParameterLocation.Query, Type = typeof(DateTime))]
        [OpenApiParameter(nameof(GetSchoolVisitorRequest.IdAcademicYear), In = ParameterLocation.Query, Type = typeof(DateTime))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetSchoolVisitorResult))]
        public Task<IActionResult> GetSchoolVisitor(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _schoolVisitorHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(SchoolVisitorEndPoint.AddSchoolVisitor))]
        [OpenApiOperation(tags: _tag, Summary = "Add Visitor School")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddSchoolVisitorRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> AddSchoolVisitor(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _schoolVisitorHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(SchoolVisitorEndPoint.UpdateSchoolVisitor))]
        [OpenApiOperation(tags: _tag, Summary = "Update Visitor School")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateSchoolVisitorRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> UpdateSchoolVisitor(
           [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route)] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _schoolVisitorHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(SchoolVisitorEndPoint.DetailSchoolVisitor))]
        [OpenApiOperation(tags: _tag, Summary = "Update Visitor School")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter("id", Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(DetailSchoolVisitorResult))]
        public Task<IActionResult> DetailSchoolVisitor(
       [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/{id}")] HttpRequest req, string id,
          CancellationToken cancellationToken)
        {
            return _schoolVisitorHandler.Execute(req, cancellationToken, true, "id".WithValue(id));
        }

        [FunctionName(nameof(SchoolVisitorEndPoint.DeleteSchoolVisitor))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Visitor School")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(IEnumerable<string>))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> DeleteSchoolVisitor(
       [HttpTrigger(AuthorizationLevel.Function, "Delete", Route = _route)] HttpRequest req,
       CancellationToken cancellationToken)
        {
            return _schoolVisitorHandler.Execute(req, cancellationToken);
        }
    }
}

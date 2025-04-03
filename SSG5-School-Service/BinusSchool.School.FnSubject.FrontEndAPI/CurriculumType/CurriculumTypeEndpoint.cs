using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model;
using BinusSchool.Data.Model.School.FnSubject.CurriculumType;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace BinusSchool.School.FnSubject.CurriculumType
{
    public class CurriculumTypeEndpoint
    {
        private const string _route = "school/curriculum-type";
        private const string _tag = "School Curriculum Type";

        private readonly CurriculumTypeHandler _handler;
        public CurriculumTypeEndpoint(CurriculumTypeHandler handler)
        {
            _handler = handler;
        }

        [FunctionName(nameof(CurriculumTypeEndpoint.GetCurriculumTypes))]
        [OpenApiOperation(tags: _tag, Summary = "Get Curriculum Type List")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(CollectionSchoolRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionSchoolRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionSchoolRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionSchoolRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionSchoolRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionSchoolRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionSchoolRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(CollectionSchoolRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(CodeWithIdVm))]
        public Task<IActionResult> GetCurriculumTypes(
         [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
         CancellationToken cancellationToken)
        {
            return _handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(CurriculumTypeEndpoint.GetCurriculumTypeDetail))]
        [OpenApiOperation(tags: _tag, Summary = "Get Curriculum Type Detail")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter("id", Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(DetailResult2))]
        public Task<IActionResult> GetCurriculumTypeDetail(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/{id}")] HttpRequest req,
           string id,
           CancellationToken cancellationToken)
        {
            return _handler.Execute(req, cancellationToken, true, "id".WithValue(id));
        }

        [FunctionName(nameof(CurriculumTypeEndpoint.AddCurriculumType))]
        [OpenApiOperation(tags: _tag, Summary = "Add Curriculum Type")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddCurriculumTypeRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> AddCurriculumType(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _handler.Execute(req, cancellationToken);
        }


        [FunctionName(nameof(CurriculumTypeEndpoint.UpdateCurriculumType))]
        [OpenApiOperation(tags: _tag, Summary = "Update Curriculum Type")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateCurriculumTypeRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateCurriculumType(
           [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route)] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(CurriculumTypeEndpoint.DeleteCurriculumType))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Curriculum Type")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(IEnumerable<string>), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> DeleteCurriculumType(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _handler.Execute(req, cancellationToken);
        }
    }
}

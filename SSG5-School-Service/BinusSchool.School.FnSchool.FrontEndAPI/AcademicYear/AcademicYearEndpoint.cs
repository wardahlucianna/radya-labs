using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model;
using BinusSchool.Data.Model.School.FnSchool.AcademicYear;
using BinusSchool.Data.Model.School.FnSchool.AnswerSet;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.School.FnSchool.AcademicYear
{
    public class AcademicYearEndpoint
    {
        private const string _route = "school/academicyears";
        private const string _tag = "School Academic year";

        private readonly AcademicYearHandler _handler;
        public AcademicYearEndpoint(AcademicYearHandler handler)
        {
            _handler = handler;
        }

        [FunctionName(nameof(AcademicYearEndpoint.GetAcademicyears))]
        [OpenApiOperation(tags: _tag, Summary = "Get Academic Year List")]
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
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetListAnswerSetResult))]
        public Task<IActionResult> GetAcademicyears(
         [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
         CancellationToken cancellationToken)
        {
            return _handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AcademicYearEndpoint.GetAcademicyearDetail))]
        [OpenApiOperation(tags: _tag, Summary = "Get Academic Year Detail")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter("id", Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(DetailResult2))]
        public Task<IActionResult> GetAcademicyearDetail(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/{id}")] HttpRequest req,
           string id,
           CancellationToken cancellationToken)
        {
            return _handler.Execute(req, cancellationToken, true, "id".WithValue(id));
        }

        [FunctionName(nameof(AcademicYearEndpoint.AddAcademicyear))]
        [OpenApiOperation(tags: _tag, Summary = "Add Academic Year")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddAcademicYearRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> AddAcademicyear(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _handler.Execute(req, cancellationToken);
        }


        [FunctionName(nameof(AcademicYearEndpoint.UpdateAcademicyear))]
        [OpenApiOperation(tags: _tag, Summary = "Update Academic Year")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateAcademicYearRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateAcademicyear(
           [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route)] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AcademicYearEndpoint.DeleteAcademicyear))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Academic Year")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(IEnumerable<string>), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> DeleteAcademicyear(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AcademicYearEndpoint.GetAcadyearByRange))]
        [OpenApiOperation(tags: _tag, Summary = "Get Academic Year by Date Range")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetAcadyearByRangeRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAcadyearByRangeRequest.Start), In = ParameterLocation.Query, Type = typeof(DateTime), Required = true)]
        [OpenApiParameter(nameof(GetAcadyearByRangeRequest.End), In = ParameterLocation.Query, Type = typeof(DateTime), Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetAcadyearByRangeResult[]))]
        public Task<IActionResult> GetAcadyearByRange(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-by-range")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetAcadyearByRangeHandler>();
            return handler.Execute(req, cancellationToken, true);
        }

        [FunctionName(nameof(AcademicYearEndpoint.GetAcadyearRange))]
        [OpenApiOperation(tags: _tag, Summary = "Get Academic Year Date Range")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter("id", In = ParameterLocation.Path, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetAcadyearByRangeResult))]
        public Task<IActionResult> GetAcadyearRange(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-range/{id}")] HttpRequest req,
           string id,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetAcadyearRangeHandler>();
            return handler.Execute(req, cancellationToken, true, "id".WithValue(id));
        }
    }
}

using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Data.Model.School.FnSubject.Subject;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.School.FnSubject.Subject
{
    public class SubjectEndpoint
    {
        private const string _route = "school/subject";
        private const string _tag = "School Subject";

        [FunctionName(nameof(SubjectEndpoint.GetSubjects))]
        [OpenApiOperation(tags: _tag, Summary = "Get Subject List")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetSubjectRequest.Ids), In = ParameterLocation.Query, Type = typeof(string[]))]
        [OpenApiParameter(nameof(GetSubjectRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSubjectRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSubjectRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetSubjectRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetSubjectRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSubjectRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSubjectRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetSubjectRequest.IdSchool), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSubjectRequest.IdAcadyear), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSubjectRequest.IdLevel), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSubjectRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSubjectRequest.IdPathway), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSubjectRequest.IdCurriculumType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSubjectRequest.IdSubjectGroup), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSubjectRequest.IdDepartment), In = ParameterLocation.Query,Type = typeof(string[]))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetSubjectResult))]
        public Task<IActionResult> GetSubjects(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetSubjectHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(SubjectEndpoint.GetSubjectDetail))]
        [OpenApiOperation(tags: _tag, Summary = "Get Subject Detail")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter("id", Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetSubjectDetailResult))]
        public Task<IActionResult> GetSubjectDetail(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/{id}")] HttpRequest req,
           string id,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<SubjectHandler>();
            return handler.Execute(req, cancellationToken, true, "id".WithValue(id));
        }

        [FunctionName(nameof(SubjectEndpoint.GetSubjectPathwayByCode))]
        [OpenApiOperation(tags: _tag, Summary = "Get Subject pathway for upload asc timetable")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(GetSubjectPathwayForAscTimetableRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetSubjectResult))]
        public Task<IActionResult> GetSubjectPathwayByCode(
           [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "-by-code-school")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<SubjectPathwayHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(SubjectEndpoint.AddSubject))]
        [OpenApiOperation(tags: _tag, Summary = "Add Subject")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddSubjectRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> AddSubject(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<SubjectHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(SubjectEndpoint.UpdateSubject))]
        [OpenApiOperation(tags: _tag, Summary = "Update Subject")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateSubjectRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateSubject(
           [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route)] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<SubjectHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(SubjectEndpoint.DeleteSubject))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Subject")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(IEnumerable<string>), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> DeleteSubject(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<SubjectHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(SubjectEndpoint.CopySubject))]
        [OpenApiOperation(tags: _tag, Summary = "Copy Subject")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(CopySubjectRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> CopySubject(
           [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/copy")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<CopySubjectHandler>();
            return handler.Execute(req, cancellationToken);
        }
    }
}

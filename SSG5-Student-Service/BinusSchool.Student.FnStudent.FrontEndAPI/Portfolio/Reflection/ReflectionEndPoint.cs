using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Data.Model.Student.FnStudent.Portfolio.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Student.FnStudent.Portfolio.Reflection
{
    public class ReflectionEndPoint
    {
        private const string _route = "student/portfolio";
        private const string _tag = "Reflection";

        private readonly ReflectionHandler _reflectionHandler;
        private readonly ReflectionCommentHandler _reflectionCommnetHandler;
        private readonly UpdateReflectionContentHandler _updateReflectionContentHandler;

        public ReflectionEndPoint(ReflectionHandler ReflectionHandler, ReflectionCommentHandler ReflectionCommentHandler, UpdateReflectionContentHandler updateReflectionContentHandler)
        {
            _reflectionHandler = ReflectionHandler;
            _reflectionCommnetHandler = ReflectionCommentHandler;
            _updateReflectionContentHandler = updateReflectionContentHandler;
        }

        #region reflection
        [FunctionName(nameof(ReflectionEndPoint.GetReflection))]
        [OpenApiOperation(tags: _tag, Summary = "Get Reflection")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetReflectionRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetReflectionRequest.IdStudent), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetReflectionRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetReflectionRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetReflectionRequest.Semester), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetReflectionRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetReflectionRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetReflectionRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetReflectionRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetReflectionRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetReflectionRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetReflectionResult[]))]
        public Task<IActionResult> GetReflection(
      [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/reflection")] HttpRequest req,
      CancellationToken cancellationToken)
        {
            return _reflectionHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ReflectionEndPoint.AddReflection))]
        [OpenApiOperation(tags: _tag, Summary = "Add Reflection")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddReflectionRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> AddReflection(
       [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/reflection")] HttpRequest req,
       CancellationToken cancellationToken)
        {
            return _reflectionHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ReflectionEndPoint.UpdateReflection))]
        [OpenApiOperation(tags: _tag, Summary = "Update Reflection Comment")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateReflectionRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> UpdateReflection(
       [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route + "/reflection")] HttpRequest req,
       CancellationToken cancellationToken)
        {
            return _reflectionHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ReflectionEndPoint.DeleteReflection))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Reflection Comment")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(IEnumerable<string>), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> DeleteReflection(
       [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route + "/reflection")] HttpRequest req,
       CancellationToken cancellationToken)
        {
            return _reflectionHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ReflectionEndPoint.GetDetailReflection))]
        [OpenApiOperation(tags: _tag, Summary = "Get Detail Lerning Goals Achieved")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter("id", Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetDetailReflectionResult))]
        public Task<IActionResult> GetDetailReflection(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/reflection/{id}")] HttpRequest req,
           string id,
        CancellationToken cancellationToken)
        {
            return _reflectionHandler.Execute(req, cancellationToken, true, "id".WithValue(id));
        }

        [FunctionName(nameof(ReflectionEndPoint.UpdateReflectionContent))]
        [OpenApiOperation(tags: _tag, Summary = "Update Reflection Content Have Image")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateReflectionContentRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> UpdateReflectionContent(
       [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route + "/reflection-content")] HttpRequest req,
       CancellationToken cancellationToken)
        {
            return _updateReflectionContentHandler.Execute(req, cancellationToken);
        }
        #endregion

        #region reflection comment
        [FunctionName(nameof(ReflectionEndPoint.AddReflectionComment))]
        [OpenApiOperation(tags: _tag, Summary = "Add Reflection Comment")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddReflectionCommentRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> AddReflectionComment(
       [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/reflection-comment")] HttpRequest req,
        [Queue("notification-portofolio")] ICollector<string> collector,
       CancellationToken cancellationToken)
        {
            return _reflectionCommnetHandler.Execute(req, cancellationToken, keyValues: nameof(collector).WithValue(collector));
        }

        [FunctionName(nameof(ReflectionEndPoint.UpdateReflectionComment))]
        [OpenApiOperation(tags: _tag, Summary = "Update Reflection Comment")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateReflectionCommentRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> UpdateReflectionComment(
       [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route + "/reflection-comment")] HttpRequest req,
       CancellationToken cancellationToken)
        {
            return _reflectionCommnetHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ReflectionEndPoint.DeleteReflectionComment))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Reflection Comment")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(IEnumerable<string>), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> DeleteReflectionComment(
       [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route + "/reflection-comment")] HttpRequest req,
       CancellationToken cancellationToken)
        {
            return _reflectionCommnetHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ReflectionEndPoint.GetDetailReflectionComment))]
        [OpenApiOperation(tags: _tag, Summary = "Get Detail Reflection Comment")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter("id", Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetDetailReflectionCommentResult))]
        public Task<IActionResult> GetDetailReflectionComment(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/reflection-comment/{id}")] HttpRequest req,
           string id,
        CancellationToken cancellationToken)
        {
            return _reflectionCommnetHandler.Execute(req, cancellationToken, true, "id".WithValue(id));
        }
        #endregion

    }
}

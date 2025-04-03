using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using BinusSchool.Data.Model.Student.FnStudent.Portfolio.LearningGolas;
using BinusSchool.Common.Extensions;
using BinusSchool.Data.Model.Student.FnStudent.Portfolio;
using BinusSchool.Data.Model.Student.FnStudent.Portfolio.Coursework;

namespace BinusSchool.Student.FnStudent.Portfolio.Coursework
{
    public class CourseworkEndPoint
    {
        private const string _route = "student/portfolio";
        private const string _tag = "Coursework";

        private readonly CourseworkHandler _CourseworkHandler;
        private readonly GetListCourseworkHandler _ListCourseworkHandler;
        private readonly GetListUOIHandler _ListUOIHandler;
        private readonly CourseworCommentHandler _CourseworkCommentHandler;
        private readonly AddCourseworkSeenByHandler _CourseworSeenByHandler;


        public CourseworkEndPoint
        (CourseworkHandler CourseworkHandler,
         GetListCourseworkHandler ListCourseworkHandler,
         GetListUOIHandler ListUOIHandler,
         CourseworCommentHandler CourseworkCommentHandler,
         AddCourseworkSeenByHandler AddCourseworkSeenByHandler
        )
        {
            _CourseworkHandler = CourseworkHandler;
            _ListCourseworkHandler = ListCourseworkHandler;
            _ListUOIHandler = ListUOIHandler;
            _CourseworkCommentHandler = CourseworkCommentHandler;
            _CourseworSeenByHandler = AddCourseworkSeenByHandler;
        }

        [FunctionName(nameof(CourseworkEndPoint.GetListCoursework))]
        [OpenApiOperation(tags: _tag, Summary = "Get List Coursework")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetListCourseworkRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetListCourseworkRequest.IdStudent), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetListCourseworkRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetListCourseworkRequest.Semester), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetListCourseworkRequest.Type), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetListCourseworkRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListCourseworkRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListCourseworkRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetListCourseworkRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetListCourseworkRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListCourseworkRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetListCourseworkResult[]))]
        public Task<IActionResult> GetListCoursework(
       [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/coursework")] HttpRequest req,
       CancellationToken cancellationToken)
        {
            return _ListCourseworkHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(CourseworkEndPoint.GetDetailCoursework))]
        [OpenApiOperation(tags: _tag, Summary = "Get Detail Coursework")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter("id", Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetDetailCourseworkResult))]
        public Task<IActionResult> GetDetailCoursework(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/coursework/{id}")] HttpRequest req,
           string id,
        CancellationToken cancellationToken)
        {
            return _CourseworkHandler.Execute(req, cancellationToken, true, "id".WithValue(id));
        }

        [FunctionName(nameof(CourseworkEndPoint.AddCoursework))]
        [OpenApiOperation(tags: _tag, Summary = "Add Coursework")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddCourseworkRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> AddCoursework(
       [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/coursework")] HttpRequest req,
        [Queue("notification-portofolio")] ICollector<string> collector,
       CancellationToken cancellationToken)
        {
            return _CourseworkHandler.Execute(req, cancellationToken, keyValues: nameof(collector).WithValue(collector));
        }

        [FunctionName(nameof(CourseworkEndPoint.UpdateCoursework))]
        [OpenApiOperation(tags: _tag, Summary = "Update Coursework")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateCourseworkRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> UpdateCoursework(
       [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route + "/coursework")] HttpRequest req,
       CancellationToken cancellationToken)
        {
            return _CourseworkHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(CourseworkEndPoint.DeleteCoursework))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Coursework")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(IEnumerable<string>), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> DeleteCoursework(
        [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route + "/coursework")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _CourseworkHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(CourseworkEndPoint.AddCourseworkComment))]
        [OpenApiOperation(tags: _tag, Summary = "Add Coursework Comment")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddCourseworkCommentRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> AddCourseworkComment(
       [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/coursework-comment")] HttpRequest req,
        [Queue("notification-portofolio")] ICollector<string> collector,
       CancellationToken cancellationToken)
        {
            return _CourseworkCommentHandler.Execute(req, cancellationToken, keyValues: nameof(collector).WithValue(collector));
        }

        [FunctionName(nameof(CourseworkEndPoint.UpdateCourseworkComment))]
        [OpenApiOperation(tags: _tag, Summary = "Update Coursework Comment")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateCourseworkCommentRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> UpdateCourseworkComment(
       [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route + "/coursework-comment")] HttpRequest req,
       CancellationToken cancellationToken)
        {
            return _CourseworkCommentHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(CourseworkEndPoint.DeleteCourseworkComment))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Coursework Comment")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(IEnumerable<string>), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> DeleteCourseworkComment(
        [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route + "/coursework-comment")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _CourseworkCommentHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(CourseworkEndPoint.AddCourseworkSeenBy))]
        [OpenApiOperation(tags: _tag, Summary = "Add Coursework Seen By")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddCourseworkSeenByRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> AddCourseworkSeenBy(
       [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/coursework-seen-by")] HttpRequest req,
       CancellationToken cancellationToken)
        {
            return _CourseworSeenByHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(CourseworkEndPoint.GetListUOI))]
        [OpenApiOperation(tags: _tag, Summary = "Get List OUI")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetListUOIRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListUOIRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListUOIRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetListUOIRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetListUOIRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListUOIRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetListUOIResult[]))]
        public Task<IActionResult> GetListUOI(
       [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/data-uoi")] HttpRequest req,
       CancellationToken cancellationToken)
        {
            return _ListUOIHandler.Execute(req, cancellationToken);
        }
    }
}

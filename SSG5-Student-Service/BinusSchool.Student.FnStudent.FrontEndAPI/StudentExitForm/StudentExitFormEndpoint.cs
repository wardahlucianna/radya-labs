using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Data.Model.Student.FnStudent.StudentExitForm;
using BinusSchool.Data.Model.Student.FnStudent.StudentExitHistory;
using BinusSchool.Data.Model.Student.FnStudent.StudentExitReason;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Student.FnStudent.StudentExitForm
{
    public class StudentExitFormEndpoint
    {
        private const string _route = "student/student-exit-form";
        private const string _tag = "Student Exit Form";

        //list
        [FunctionName(nameof(StudentExitFormEndpoint.GetListStudentExitForm))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetStudentExitFormRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetStudentExitFormRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetStudentExitFormRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetStudentExitFormRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetStudentExitFormRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetStudentExitFormRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetStudentExitFormRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetStudentExitFormRequest.IdUser), In = ParameterLocation.Query, Type = typeof(string), Required =true)]
        [OpenApiParameter(nameof(GetStudentExitFormRequest.IdSchool), In = ParameterLocation.Query, Type = typeof(string), Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetStudentExitFormResult))]
        public Task<IActionResult> GetListStudentExitForm(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<StudentExitFormHandler>();
            return handler.Execute(req, cancellationToken, false);
        }

        //add
        [FunctionName(nameof(StudentExitFormEndpoint.AddStudentExitForm))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddStudentExitFormRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> AddStudentExitForm(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route)] HttpRequest req,
            [Queue("notification-student-exit")] ICollector<string> collector,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<StudentExitFormHandler>();
            return handler.Execute(req, cancellationToken, keyValues: nameof(collector).WithValue(collector));
        }

        //update
        [FunctionName(nameof(StudentExitFormEndpoint.UpdateStudentExitForm))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateStudentExitFormRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateStudentExitForm(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<StudentExitFormHandler>();
            return handler.Execute(req, cancellationToken);
        }

        //detail
        [FunctionName(nameof(StudentExitFormEndpoint.GetStudentExitFormDetail))]
        [OpenApiOperation(tags: _tag, Summary = "Get Student Exit Form Detail")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter("id", Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetStudentExitFormDetailResult))]
        public Task<IActionResult> GetStudentExitFormDetail(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/detail/{id}")] HttpRequest req,
            string id,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<StudentExitFormHandler>();
            return handler.Execute(req, cancellationToken, keyValues: "id".WithValue(id));
        }

        //delete
        [FunctionName(nameof(StudentExitFormEndpoint.DeleteStudentExitForm))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Student Exit Form")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(IEnumerable<string>), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> DeleteStudentExitForm(
        [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route)] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<StudentExitFormHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(StudentExitFormEndpoint.GetListStudentExitReasons))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetStudentExitReasonRequest.IdStudentExitReason), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetStudentExitReasonRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetStudentExitReasonRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetStudentExitReasonRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetStudentExitReasonRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetStudentExitReasonRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetStudentExitReasonRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetStudentExitReasonRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetStudentExitReasonResult))]
        public Task<IActionResult> GetListStudentExitReasons(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/list-reason")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetListStudentExitReasonHandler>();
            return handler.Execute(req, cancellationToken, false);
        }

        [FunctionName(nameof(StudentExitFormEndpoint.GetListStudentExitHistories))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetStudentExitHistoryRequest.IdStudentExit), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetStudentExitHistoryResult))]
        public Task<IActionResult> GetListStudentExitHistories(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/list-history")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetListStudentExitHistoryHandler>();
            return handler.Execute(req, cancellationToken, false);
        }

        [FunctionName(nameof(StudentExitFormEndpoint.GetParentByChild))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetParentByChildRequest.IdStudent), In = ParameterLocation.Query, Required =true)]
        [OpenApiParameter(nameof(GetParentByChildRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetParentByChildRequest.Semester), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetParentByChildResult))]
        public Task<IActionResult> GetParentByChild(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/parent-by-child")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetParentByChildHandler>();
            return handler.Execute(req, cancellationToken, false);
        }

        [FunctionName(nameof(StudentExitFormEndpoint.GetAccessStudentExit))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetAccessStudentExitFormRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAccessStudentExitFormRequest.IdParent), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetAccessStudentExitFormResult))]
        public Task<IActionResult> GetAccessStudentExit(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/access")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetAccessStudentExitFormHandler>();
            return handler.Execute(req, cancellationToken, false);
        }
    }
}

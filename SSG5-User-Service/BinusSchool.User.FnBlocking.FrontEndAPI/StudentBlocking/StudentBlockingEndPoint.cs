using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Data.Model.User.FnBlocking.StudentBlocking;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.User.FnBlocking.StudentBlocking
{
    public class StudentBlockingEndPoint
    {
        private const string _route = "user-blocking/student-blocking";
        private const string _tag = "Student Blocking";

        [FunctionName(nameof(StudentBlockingEndPoint.UploadExcelColumnNameStudentBlockingValidation))]
        [OpenApiOperation(tags: _tag, Summary = "Upload Excel Columns Name Student Blocking Validation")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(UploadExcelColumnNameStudentBlockingValidationRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(UploadExcelColumnNameStudentBlockingValidationResult))]
        public Task<IActionResult> UploadExcelColumnNameStudentBlockingValidation(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "-validation-excel-column-name")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<UploadExcelColumnNameStudentBlockingValidationHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(StudentBlockingEndPoint.UploadExcelContentDataStudentBlockingValidation))]
        [OpenApiOperation(tags: _tag, Summary = "Upload Excel Content Data Student Blocking Validation")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(UploadExcelContentDataStudentBlockingValidationRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        //[OpenApiParameter(nameof(UploadExcelContentDataStudentBlockingValidationRequest.Search), In = ParameterLocation.Query)]
        //[OpenApiParameter(nameof(UploadExcelContentDataStudentBlockingValidationRequest.Return), In = ParameterLocation.Query)]
        //[OpenApiParameter(nameof(UploadExcelContentDataStudentBlockingValidationRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        //[OpenApiParameter(nameof(UploadExcelContentDataStudentBlockingValidationRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        //[OpenApiParameter(nameof(UploadExcelContentDataStudentBlockingValidationRequest.OrderBy), In = ParameterLocation.Query)]
        //[OpenApiParameter(nameof(UploadExcelContentDataStudentBlockingValidationRequest.OrderType), In = ParameterLocation.Query)]
        //[OpenApiParameter(nameof(UploadExcelContentDataStudentBlockingValidationRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        //[OpenApiRequestBody("multipart/form-data", typeof(UploadExcelContentDataStudentBlockingValidationRequest))]
        //[OpenApiRequestBody("multipart/form-data", typeof(IFormFile), Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(object))]
        public Task<IActionResult> UploadExcelContentDataStudentBlockingValidation(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "-validation-excel-content-data")] HttpRequest req,
        [Queue("notification-bp-blockingstudent")] ICollector<string> collector,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<UploadExcelContentDataStudentBlockingValidationHandler>();
            return handler.Execute(req, cancellationToken, keyValues: nameof(collector).WithValue(collector));
        }

        [FunctionName(nameof(StudentBlockingEndPoint.GetColumnNameStudentBlocking))]
        [OpenApiOperation(tags: _tag, Summary = "Get Column Name Student Blocking")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetColumnNameStudentBlockingRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetColumnNameStudentBlockingResult))]
        public Task<IActionResult> GetColumnNameStudentBlocking(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-column-name")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetColumnNameStudentBlockingHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(StudentBlockingEndPoint.GetContentDataStudentBlocking))]
        [OpenApiOperation(tags: _tag, Summary = "Get Content Data Student Blocking")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetContentDataStudentBlockingRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetContentDataStudentBlockingRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetContentDataStudentBlockingRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetContentDataStudentBlockingRequest.IdLevel), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetContentDataStudentBlockingRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetContentDataStudentBlockingRequest.Semester), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetContentDataStudentBlockingRequest.IdHoomRoom), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetContentDataStudentBlockingRequest.IdStudent), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetContentDataStudentBlockingRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetContentDataStudentBlockingRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetContentDataStudentBlockingRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetContentDataStudentBlockingRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetContentDataStudentBlockingRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetContentDataStudentBlockingRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetContentDataStudentBlockingRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(object))]
        public Task<IActionResult> GetContentDataStudentBlocking(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-content-data")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetContentDataStudentBlockingHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(StudentBlockingEndPoint.GetAccessBlockStudentBlocking))]
        [OpenApiOperation(tags: _tag, Summary = "Get Access Block Student Blocking")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetAccessBlockStudentBlockingResult))]
        public Task<IActionResult> GetAccessBlockStudentBlocking(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-access-block")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetAccessBlockStudentBlockingHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(StudentBlockingEndPoint.AddStudentBlocking))]
        [OpenApiOperation(tags: _tag, Summary = "Add Student Blocking")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddStudentBlockingRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> AddStudentBlocking(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route)] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<AddStudentBlockingHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(StudentBlockingEndPoint.UpdateStudentBlocking))]
        [OpenApiOperation(tags: _tag, Summary = "Update Student Blocking")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateStudentBlockingRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateStudentBlocking(
        [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route)] HttpRequest req,
        [Queue("notification-bp-blockingstudent")] ICollector<string> collector,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<UpdateStudentBlockingHandler>();
            return handler.Execute(req, cancellationToken, keyValues: nameof(collector).WithValue(collector));
        }

        [FunctionName(nameof(StudentBlockingEndPoint.DownloadDocumentStudentBlocking))]
        [OpenApiOperation(tags: _tag, Summary = "Download Document Student Blocking")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(DownloadExcelStudentBlockingRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(DownloadExcelStudentBlockingRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(DownloadExcelStudentBlockingRequest))]
        public Task<IActionResult> DownloadDocumentStudentBlocking(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-download-excel")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<DownloadExcelStudentBlockingHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(StudentBlockingEndPoint.GetStudentBlockingCategoryType))]
        [OpenApiOperation(tags: _tag, Summary = "Get Student Blocking Category Type")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetStudentBlockingCategoryTypeRequest.IdStudent), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetStudentBlockingCategoryTypeResult>))]
        public Task<IActionResult> GetStudentBlockingCategoryType(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-category-type")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetStudentBlockingCategoryTypeHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(StudentBlockingEndPoint.UpdateStudentUnBlocking))]
        [OpenApiOperation(tags: _tag, Summary = "Update Student UnBlocking")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateStudentUnBlockingRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateStudentUnBlocking(
        [HttpTrigger(AuthorizationLevel.Function, "put", Route = "user-blocking/student-unblocking")] HttpRequest req,
        [Queue("notification-bp-blockingstudent")] ICollector<string> collector,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<UpdateStudentUnBlockingHandler>();
            return handler.Execute(req, cancellationToken, keyValues: nameof(collector).WithValue(collector));
        }

        [FunctionName(nameof(StudentBlockingEndPoint.GetDataStudentBlocking))]
        [OpenApiOperation(tags: _tag, Summary = "Get Data Student Blocking")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetDataStudentBlockingRequest.IdBlockingCategory), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDataStudentBlockingRequest.IdBlockingType), In = ParameterLocation.Query, Required = false)]
        [OpenApiParameter(nameof(GetDataStudentBlockingRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDataStudentBlockingRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDataStudentBlockingRequest.IdLevel), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDataStudentBlockingRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDataStudentBlockingRequest.Semester), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDataStudentBlockingRequest.IdHoomRoom), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDataStudentBlockingRequest.IdStudent), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDataStudentBlockingRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDataStudentBlockingRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDataStudentBlockingRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetDataStudentBlockingRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetDataStudentBlockingRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDataStudentBlockingRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDataStudentBlockingRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetDataStudentBlockingResult))]
        public Task<IActionResult> GetDataStudentBlocking(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetDataStudentBlockingHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(StudentBlockingEndPoint.DownloadDocumentStudentBlockingByCategory))]
        [OpenApiOperation(tags: _tag, Summary = "Download Document Student Blocking By Category")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(DownloadExcelStudentBlockingByCategoryRequest.IdBlockingCategory), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(DownloadExcelStudentBlockingByCategoryRequest.IdBlockingType), In = ParameterLocation.Query, Required = false)]
        [OpenApiParameter(nameof(DownloadExcelStudentBlockingByCategoryRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(DownloadExcelStudentBlockingByCategoryRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(DownloadExcelStudentBlockingByCategoryRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(DownloadExcelStudentBlockingByCategoryRequest.Semester), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(DownloadExcelStudentBlockingByCategoryResult))]
        public Task<IActionResult> DownloadDocumentStudentBlockingByCategory(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-download-excel-category")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<DownloadExcelStudentBlockingByCategoryHandler>();
            return handler.Execute(req, cancellationToken);
        }
    }
}

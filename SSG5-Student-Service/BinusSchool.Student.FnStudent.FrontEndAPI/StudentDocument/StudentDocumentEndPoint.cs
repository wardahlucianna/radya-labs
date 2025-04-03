using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.StudentDocument;
using BinusSchool.Student.FnStudent.Student;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Student.FnStudent.StudentDocument
{
    public class StudentDocumentEndPoint
    {

        private const string _route = "student/Document";
        private const string _tag = "StudentDocument";

        [FunctionName(nameof(StudentDocumentEndPoint.GetDocumentType))]
        [OpenApiOperation(tags: _tag, Summary = "Get document Type")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(CollectionRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetDocumentTypeResult))]
        public Task<IActionResult> GetDocumentType(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/GetDocumentType")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetDocumentTypeHandler>();
            return handler.Execute(req, cancellationToken,false);
        }

        [FunctionName(nameof(StudentDocumentEndPoint.GetAdmissionDocumentByStudentID))]
        [OpenApiOperation(tags: _tag, Summary = "Get Admission Document By StudentID")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter("id", Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetDocumentByStudentResult))]
        public Task<IActionResult> GetAdmissionDocumentByStudentID(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/GetAdmissionDocument/{id}")] HttpRequest req,
           string id,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<AdmissionDocumentByStudentIDHandler>();
            return handler.Execute(req, cancellationToken, true, "id".WithValue(id));
        }

        [FunctionName(nameof(StudentDocumentEndPoint.GetAdditionalDocumentByStudentID))]
        [OpenApiOperation(tags: _tag, Summary = "Get Additional Document By StudentID")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(AdditionalDocumentByStudentIDRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(AdditionalDocumentByStudentIDRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(AdditionalDocumentByStudentIDRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(AdditionalDocumentByStudentIDRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(AdditionalDocumentByStudentIDRequest.Role), In = ParameterLocation.Query)]
        [OpenApiParameter("id", Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetDocumentByStudentResult))]
        public Task<IActionResult> GetAdditionalDocumentByStudentID(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/GetAdditionalDocumentByStudentID/{id}")] HttpRequest req,
           string id,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<AdditionalDocumentByStudentIDHandler>();
            return handler.Execute(req, cancellationToken, true, "id".WithValue(id));
        }

        [FunctionName(nameof(StudentDocumentEndPoint.AddStudentDocument))]
        [OpenApiOperation(tags: _tag, Summary = "Add Student Document")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        //[OpenApiRequestBody("application/json", typeof(AddStudentDocumentRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(AddStudentDocumentRequest))]
        public Task<IActionResult> AddStudentDocument(
           [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/AddStudentDocument")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<AdmissionDocumentByStudentIDHandler>();
            return handler.Execute(req, cancellationToken, true);
        }

        [FunctionName(nameof(StudentDocumentEndPoint.UpdateStudentDocument))]
        [OpenApiOperation(tags: _tag, Summary = "Update Student Document")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        //[OpenApiRequestBody("application/json", typeof(UpdateStudentDocumentRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateStudentDocument(
           [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route + "/UpdateStudentDocument")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<AdmissionDocumentByStudentIDHandler>();
            return handler.Execute(req, cancellationToken, true);
        }

        [FunctionName(nameof(StudentDocumentEndPoint.DeleteStudentDocument))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Student Document")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(IEnumerable<string>), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> DeleteStudentDocument(
           [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route + "/DeleteStudentDocument")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<AdmissionDocumentByStudentIDHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(StudentDocumentEndPoint.GetStudentDocumentByStudentIDFile))]
        [OpenApiOperation(tags: _tag, Summary = "GetStudentDocumentByStudentIDFile")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        //[OpenApiRequestBody("application/json", typeof(GetStudentDocumentFileRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> GetStudentDocumentByStudentIDFile(
           [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/GetStudentDocumentByStudentIDFile")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetStudentDocumentByStudentIDFileHandler>();
            return handler.Execute(req, cancellationToken);
        }

    }
}

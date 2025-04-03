using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model;
using BinusSchool.Data.Model.Student.FnStudent.StudentProfileConfirmation;
using BinusSchool.Student.FnStudent.SiblingGroup;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Student.FnStudent.StudentProfileConfirmation
{
    public class StudentProfileConfirmationEndPoint
    {
        private const string _route = "student/profile_confirmation";
        private const string _tag = "Profile Confirmation";

        private readonly StudentProfileConfirmationHandler _studentProfileConfirmationHandler;
        public StudentProfileConfirmationEndPoint(StudentProfileConfirmationHandler studentProfileConfirmationHandler)
        {
            _studentProfileConfirmationHandler = studentProfileConfirmationHandler;
        }

        [FunctionName(nameof(StudentProfileConfirmationEndPoint.GetStudentProfileConfirmations))]
        [OpenApiOperation(tags: _tag, Summary = "Get Sibling Group List")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(CollectionSchoolRequest.IdSchool), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetStudentProfileConfirmationResult))]
        public Task<IActionResult> GetStudentProfileConfirmations(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _studentProfileConfirmationHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(StudentProfileConfirmationEndPoint.GetStudentProfileConfirmationDetail))]
        [OpenApiOperation(tags: _tag, Summary = "Get SiblingGroup Detail")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter("id", Required = true)]
        //[OpenApiParameter(nameof(GetSiblingGroupRequest.IdStudent), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetStudentProfileConfirmationResult))]
        public Task<IActionResult> GetStudentProfileConfirmationDetail(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/{id}")] HttpRequest req,
           string id,
           CancellationToken cancellationToken)
        {
            return _studentProfileConfirmationHandler.Execute(req, cancellationToken, true, "id".WithValue(id));
        }

        [FunctionName(nameof(StudentProfileConfirmationEndPoint.AddStudentProfileConfirmation))]
        [OpenApiOperation(tags: _tag, Summary = "Add SiblingGroup")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddStudentProfileConfirmationRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> AddStudentProfileConfirmation(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _studentProfileConfirmationHandler.Execute(req, cancellationToken);
        }


        [FunctionName(nameof(StudentProfileConfirmationEndPoint.UpdateStudentProfileConfirmation))]
        [OpenApiOperation(tags: _tag, Summary = "Update SiblingGroup")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddStudentProfileConfirmationRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateStudentProfileConfirmation(
           [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route)] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _studentProfileConfirmationHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(StudentProfileConfirmationEndPoint.DeleteStudentProfileConfirmation))]
        [OpenApiOperation(tags: _tag, Summary = "Delete SiblingGroup")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(IEnumerable<string>), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> DeleteStudentProfileConfirmation(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _studentProfileConfirmationHandler.Execute(req, cancellationToken);
        }
    }
}

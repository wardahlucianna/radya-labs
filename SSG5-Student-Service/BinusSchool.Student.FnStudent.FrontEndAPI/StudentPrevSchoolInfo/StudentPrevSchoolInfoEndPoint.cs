using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model;
using BinusSchool.Data.Model.Student.FnStudent.StudentPrevSchoolInfo;
using BinusSchool.Student.FnStudent.SiblingGroup;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Student.FnStudent.StudentPrevSchoolInfo
{
    public class StudentPrevSchoolInfoEndPoint
    {
        private const string _route = "student/Student-PrevSchool-Info";
        private const string _tag = "Student PrevSchool Info";

        private readonly StudentPrevSchoolInfoHandler _studentPrevSchoolInfoHandler;
        public StudentPrevSchoolInfoEndPoint(StudentPrevSchoolInfoHandler studentPrevSchoolInfoHandler)
        {
            _studentPrevSchoolInfoHandler = studentPrevSchoolInfoHandler;
        }

        [FunctionName(nameof(StudentPrevSchoolInfoEndPoint.GetStudentPrevSchoolInfos))]
        [OpenApiOperation(tags: _tag, Summary = "Get Student PrevSchool Info List")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(CollectionSchoolRequest.IdSchool), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetStudentPrevSchoolInfoResult))]
        public Task<IActionResult> GetStudentPrevSchoolInfos(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _studentPrevSchoolInfoHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(StudentPrevSchoolInfoEndPoint.GetStudentPrevSchoolInfoDetail))]
        [OpenApiOperation(tags: _tag, Summary = "Get Student PrevSchool Info Detail")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter("id", Required = true)]
        //[OpenApiParameter(nameof(GetSiblingGroupRequest.IdStudent), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetStudentPrevSchoolInfoResult))]
        public Task<IActionResult> GetStudentPrevSchoolInfoDetail(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/{id}")] HttpRequest req,
           string id,
           CancellationToken cancellationToken)
        {
            return _studentPrevSchoolInfoHandler.Execute(req, cancellationToken, true, "id".WithValue(id));
        }

        [FunctionName(nameof(StudentPrevSchoolInfoEndPoint.UpdateStudentPrevSchoolInfo))]
        [OpenApiOperation(tags: _tag, Summary = "Update Student Prev School Info")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateStudentPrevSchoolInfoRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateStudentPrevSchoolInfo(
           [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route)] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _studentPrevSchoolInfoHandler.Execute(req, cancellationToken);
        }

    }
}

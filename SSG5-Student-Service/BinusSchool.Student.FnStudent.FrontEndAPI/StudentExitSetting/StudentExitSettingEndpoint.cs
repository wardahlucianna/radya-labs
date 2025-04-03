using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.StudentExitSetting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Primitives;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Student.FnStudent.StudentExitSetting
{
    public class StudentExitSettingEndpoint
    {
        private const string _route = "student/student-exit-setting";
        private const string _tag = "Student";
        private readonly GetAllStudentExitSettingHandler _getAllStudentExitSettingHandler;
        private readonly UpdateStudentExitSettingHandler _updateStudentExitSettingHandler;

        public StudentExitSettingEndpoint(GetAllStudentExitSettingHandler getAllStudentExitSettingHandler,
            UpdateStudentExitSettingHandler updateStudentExitSettingHandler)
        {
            _getAllStudentExitSettingHandler = getAllStudentExitSettingHandler;
            _updateStudentExitSettingHandler = updateStudentExitSettingHandler;
        }

        [FunctionName(nameof(StudentExitSettingEndpoint.GetAllStudentExitSetting))]
        [OpenApiOperation(tags: _tag, Summary = "Get All Student Exit Setting")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(CollectionRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetAllStudentExitSettingRequest.AcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAllStudentExitSettingRequest.Semester), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAllStudentExitSettingRequest.IdLevel), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAllStudentExitSettingRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAllStudentExitSettingRequest.IdHomeroom), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAllStudentExitSettingRequest.IsExit), In = ParameterLocation.Query, Type = typeof(bool), Required = false)]
        public Task<IActionResult> GetAllStudentExitSetting(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _getAllStudentExitSettingHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(StudentExitSettingEndpoint.UpdateStudentExitSetting))]
        [OpenApiOperation(tags: _tag, Summary = "Update Student Exit Setting")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateStudentExitSettingRequest))]
        [OpenApiResponseWithoutBody(System.Net.HttpStatusCode.OK)]
        public Task<IActionResult> UpdateStudentExitSetting(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _updateStudentExitSettingHandler.Execute(req, cancellationToken);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.ClassDiary;
using BinusSchool.Data.Model.Scheduling.FnSchedule.RolePosition;
using BinusSchool.Data.Model.Student.FnGuidanceCounseling.ReportStudentToGc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Scheduling.FnSchedule.RolePosition
{
    public class RolePositionEndpoint
    {
        private const string _route = "schedule/role-position";
        private const string _tag = "Role Position";

       /// <summary>
       /// api untuk mendapatkan position by user. biasanya digunakan untuk ddl position di setiap fitur
       /// </summary>
       /// <param name="req"></param>
       /// <param name="cancellationToken"></param>
       /// <returns></returns>
        [FunctionName(nameof(RolePositionEndpoint.GetPositionByUser))]
        [OpenApiOperation(tags: _tag, Summary = "Get Position By User")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetPositionByUserRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetPositionByUserRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(CodeWithIdVm[]))]
        public Task<IActionResult> GetPositionByUser(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
          CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetPositionByUserHandler>();
            return handler.Execute(req, cancellationToken);
        }

        /// <summary>
        /// api untuk mendapatkan Subject by user. biasanya digunakan untuk mendapatkan data list dengan parameter IdTeacherPosition
        /// </summary>
        /// <param name="req"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [FunctionName(nameof(RolePositionEndpoint.GetSubjectByUser))]
        [OpenApiOperation(tags: _tag, Summary = "Get Subject By User")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(GetSubjectByUserRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetSubjectByUserResult[]))]
        public Task<IActionResult> GetSubjectByUser(
         [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route+"/subject")] HttpRequest req,
         CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetSubjectByUserHandler>();
            return handler.Execute(req, cancellationToken);
        }

        /// <summary>
        /// api untuk mendapatkan user by role dan position. biasanya digunakan untuk mendapatkan IdUser seperti di event school respondent
        /// </summary>
        /// <param name="req"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [FunctionName(nameof(RolePositionEndpoint.GetUserRolePosition))]
        [OpenApiOperation(tags: _tag, Summary = "Get User By Role Position")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(GetUserRolePositionRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetUserRolePositionResult[]))]
        public Task<IActionResult> GetUserRolePosition(
         [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/user")] HttpRequest req,
         CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetUserRolePositionHandler>();
            return handler.Execute(req, cancellationToken);
        }

        /// <summary>
        /// api untuk mendapatkan user by role dan position. biasanya digunakan untuk mendapatkan IdUser seperti di event school respondent
        /// </summary>
        /// <param name="req"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [FunctionName(nameof(RolePositionEndpoint.GetUserSubjectByEmailRecepient))]
        [OpenApiOperation(tags: _tag, Summary = "Get User Subject By Email Recepient")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(GetUserSubjectByEmailRecepientRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetUserSubjectByEmailRecepientResult[]))]
        public Task<IActionResult> GetUserSubjectByEmailRecepient(
         [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/user-email-recepient")] HttpRequest req,
         CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetUserSubjectByEmailRecepientHandler>();
            return handler.Execute(req, cancellationToken);
        }
    }
}

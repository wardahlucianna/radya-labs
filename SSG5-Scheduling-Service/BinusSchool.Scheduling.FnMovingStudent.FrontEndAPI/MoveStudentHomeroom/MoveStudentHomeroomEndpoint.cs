using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnMovingStudent.MovingStudentHomeroom;
using BinusSchool.Scheduling.FnMovingStudent.MoveStudentEnrollment;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Scheduling.FnMovingStudent.MoveStudentHomeroom
{
    public class MoveStudentHomeroomEndpoint
    {
        private const string _route = "moving-student/move-student-homeroom";
        private const string _tag = "Move Student Homeroom";

        public MoveStudentHomeroomEndpoint()
        {
        }

        [FunctionName(nameof(MoveStudentHomeroomEndpoint.GetHomeroomNewMoveStudentHomeroom))]
        [OpenApiOperation(tags: _tag, Summary = "Get Homeroom Move Student Subject")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetHomeroomNewMoveStudentHomeroomRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetHomeroomNewMoveStudentHomeroomRequest.Semester), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetHomeroomNewMoveStudentHomeroomRequest.IdGradeOld), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetHomeroomNewMoveStudentHomeroomRequest.IdHomeroomOld), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(ItemValueVm[]))]
        public Task<IActionResult> GetHomeroomNewMoveStudentHomeroom(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/homeroom-new")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetHomeroomNewMoveStudentHomeroomHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MoveStudentHomeroomEndpoint.GetStudentMoveStudentHomeroom))]
        [OpenApiOperation(tags: _tag, Summary = "Get Student Move Student Subject")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetStudentMoveStudentHomeroomRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetStudentMoveStudentHomeroomRequest.Semester), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetStudentMoveStudentHomeroomRequest.IdHomeroomOld), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetStudentMoveStudentHomeroomResult[]))]
        public Task<IActionResult> GetStudentMoveStudentHomeroom(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/student")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetStudentMoveStudentHomeroomHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MoveStudentHomeroomEndpoint.AddStudentMoveStudentHomeroom))]
        [OpenApiOperation(tags: _tag, Summary = "Add Move Student Subject")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddStudentMoveStudentHomeroomRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> AddStudentMoveStudentHomeroom(
           [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route)] HttpRequest req,
           [Queue("notification-ens")] ICollector<string> collector,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<AddStudentMoveStudentHomeroomHandler>();
            return handler.Execute(req, cancellationToken, keyValues: nameof(collector).WithValue(collector));
        }

        [FunctionName(nameof(MoveStudentHomeroomEndpoint.HistoryMoveStudentHomeroom))]
        [OpenApiOperation(tags: _tag, Summary = "History Move Student Subject")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(HistoryMoveStudentHomeroomRequest.IdHomeroomStudent), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(HistoryMoveStudentHomeroomResult[]))]
        public Task<IActionResult> HistoryMoveStudentHomeroom(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/history")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<HistoryMoveStudentHomeroomHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MoveStudentHomeroomEndpoint.MoveHomeroomSync))]
        [OpenApiOperation(tags: _tag, Summary = "Move Homeroom Sync")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(MoveHomeroomSyncRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> MoveHomeroomSync(
           [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route + "/sync")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<MoveHomeroomSyncHandler>();
            return handler.Execute(req, cancellationToken);
        }

        //[FunctionName(nameof(MoveStudentHomeroomEndpoint.MoveHomeroomRepairData))]
        //[OpenApiOperation(tags: _tag, Summary = "Move Homeroom repair data")]
        //[OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        //[OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        //[OpenApiParameter(nameof(MoveHomeroomRepairDataRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        //[OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        //public Task<IActionResult> MoveHomeroomRepairData(
        //   [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/repair-data")] HttpRequest req,
        //   CancellationToken cancellationToken)
        //{
        //    var handler = req.HttpContext.RequestServices.GetService<MoveHomeroomRepairDataHandler>();
        //    return handler.Execute(req, cancellationToken);
        //}
    }
}

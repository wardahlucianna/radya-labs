using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.ElectiveCoachAttendance;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Scheduling.FnExtracurricular.ElectiveCoachAttendance
{
    public class ElectiveCoachAttendanceEndpoint
    {
        private const string _route = "extracurricular/coach-att";
        private const string _tag = "Extracurricular Coach Attendance";

        private readonly AddElectiveCoachAttendanceHandler _addElectiveCoachAttendanceHandler;
        private readonly GetElectiveCoachAttendanceHandler _getElectiveCoachAttendanceHandler;


        public ElectiveCoachAttendanceEndpoint(AddElectiveCoachAttendanceHandler addElectiveCoachAttendanceHandler,
            GetElectiveCoachAttendanceHandler getElectiveCoachAttendanceHandler)
        {
            _addElectiveCoachAttendanceHandler = addElectiveCoachAttendanceHandler;
            _getElectiveCoachAttendanceHandler = getElectiveCoachAttendanceHandler;
        }

        [FunctionName(nameof(ElectiveCoachAttendanceEndpoint.AddElectiveCoachAttendance))]
        [OpenApiOperation(tags: _tag, Summary = "Add Elective Coach Attendance")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddElectiveCoachAttendanceRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(AddElectiveCoachAttendanceResult))]
        public Task<IActionResult> AddElectiveCoachAttendance(
          [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/add-coach-attendace")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            return _addElectiveCoachAttendanceHandler.Execute(req, cancellationToken);
        }


        [FunctionName(nameof(ElectiveCoachAttendanceEndpoint.GetElectiveCoachAttendance))]
        [OpenApiOperation(tags: _tag, Summary = "Get Elective Coach Attendance")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]        
        [OpenApiParameter(nameof(CollectionRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetElectiveCoachAttendanceRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetElectiveCoachAttendanceRequest.Semester), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetElectiveCoachAttendanceRequest.IdUser), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetElectiveCoachAttendanceResult>))]
        public Task<IActionResult> GetElectiveCoachAttendance(
      [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-coach-attendace")] HttpRequest req,
      CancellationToken cancellationToken)
        {
            return _getElectiveCoachAttendanceHandler.Execute(req, cancellationToken);
        }
    }
}

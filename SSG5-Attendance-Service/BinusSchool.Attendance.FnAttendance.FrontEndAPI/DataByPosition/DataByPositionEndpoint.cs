using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Attendance.FnAttendance.LevelByPosition;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Attendance.FnAttendance.DataByPosition;
using BinusSchool.Data.Model.Attendance.FnAttendance.GradeByPosition;
using BinusSchool.Data.Model.Attendance.FnAttendance.HomeroomByPosition;
using BinusSchool.Data.Model.Attendance.FnAttendance.LevelByPosition;
using BinusSchool.Data.Model.Scoring.FnScoring.Filter;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Attendance.FnAttendance.DataByPosition
{
    public class DataByPositionEndpoint
    {
        private const string _route = "attendance";
        private const string _tag = "Data By Position";


        [FunctionName(nameof(DataByPositionEndpoint.LevelByPosition))]
        [OpenApiOperation(tags: _tag, Description = @"Get Data Level By Position")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(LevelByPositionRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(LevelByPositionRequest.SelectedPosition), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(LevelByPositionRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(IEnumerable<ItemValueVm>))]
        public Task<IActionResult> LevelByPosition(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/level-by-position")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<LevelByPositionHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(DataByPositionEndpoint.GradeByPosition))]
        [OpenApiOperation(tags: _tag, Description = @"Get Data Grade By Position")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GradeByPositionRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GradeByPositionRequest.SelectedPosition), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GradeByPositionRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GradeByPositionRequest.IdLevel), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(IEnumerable<ItemValueVm>))]
        public Task<IActionResult> GradeByPosition(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/grade-by-position")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GradeByPositionHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(DataByPositionEndpoint.HomeroomByPosition))]
        [OpenApiOperation(tags: _tag, Description = @"Get Data Homeroom By Position")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(HomeroomByPositionRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(HomeroomByPositionRequest.SelectedPosition), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(HomeroomByPositionRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(HomeroomByPositionRequest.IdLevel), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(HomeroomByPositionRequest.IdGrade), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(HomeroomByPositionRequest.Semester), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(IEnumerable<ItemValueVm>))]
        public Task<IActionResult> HomeroomByPosition(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/homeroom-by-position")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<HomeroomByPositionHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(DataByPositionEndpoint.GetListFilterAttendance))]
        [OpenApiOperation(tags: _tag, Summary = "Get List Filter Attendance")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetListFilterAttendanceRequest.IdUser), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListFilterAttendanceRequest.IdSchool), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListFilterAttendanceRequest.IdAcademicYear), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListFilterAttendanceRequest.ShowLevel), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetListFilterAttendanceRequest.ShowGrade), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetListFilterAttendanceRequest.ShowSemester), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetListFilterAttendanceRequest.ShowTerm), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetListFilterScoringResult))]
        public Task<IActionResult> GetListFilterAttendance(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-list-filter-attendance")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetListFilterAttendanceHandler>();
            return handler.Execute(req, cancellationToken);
        }
    }
}

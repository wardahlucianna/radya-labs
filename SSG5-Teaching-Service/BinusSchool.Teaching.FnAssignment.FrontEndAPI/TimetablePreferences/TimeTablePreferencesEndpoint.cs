using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Teaching.FnAssignment.Timetable;
using BinusSchool.Persistence.TeachingDb.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Teaching.FnAssignment.TimetablePreferences
{
    public class TimeTablePreferencesEndpoint
    {
        private const string _route = "assignment/timetable-preferences";
        private const string _tag = "Timetable Preferences";

        [FunctionName(nameof(TimeTablePreferencesEndpoint.DeleteTimeTable))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(IEnumerable<string>), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> DeleteTimeTable(
          [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route)] HttpRequest req,
          CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetRequiredService<TimeTablePreferencesHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(TimeTablePreferencesEndpoint.AddTimeTable))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddTimeTableRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> AddTimeTable(
         [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route)] HttpRequest req,
         CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetRequiredService<TimeTablePreferencesHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(TimeTablePreferencesEndpoint.PostTimeTable))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(PostTimetableRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> PostTimeTable(
         [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "-add")] HttpRequest req,
         CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetRequiredService<TimetablePreferencesPostHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(TimeTablePreferencesEndpoint.UpdateTimetable))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddTimeTableRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateTimetable(
        [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route)] HttpRequest req,
         CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetRequiredService<TimeTablePreferencesHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(TimeTablePreferencesEndpoint.GetTimetables))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetListTimeTableRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListTimeTableRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListTimeTableRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetListTimeTableRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetListTimeTableRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListTimeTableRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListTimeTableRequest.IdAcademicyears), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetListTimeTableRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetListTimeTableResult>))]
        public Task<IActionResult> GetTimetables(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetRequiredService<GetTimeTablePreferencesHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(TimeTablePreferencesEndpoint.GetTimetableStatus))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(IdCollection.Ids), In = ParameterLocation.Query, Type = typeof(string[]), Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(TimetableResult[]))]
        public Task<IActionResult> GetTimetableStatus(
          [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/status")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetRequiredService<GetTimeTablePreferencesStatusHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(TimeTablePreferencesEndpoint.GetTimetableByUser))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetTimeTableByUserRequest.IdSchoolUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetTimeTableByUserRequest.IdSchoolAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetTimeTableByUserResult))]
        public Task<IActionResult> GetTimetableByUser(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/by-user")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetRequiredService<TimeTablePreferencesHandler>();
            return handler.Execute(req, cancellationToken);
        }


        [FunctionName(nameof(TimeTablePreferencesEndpoint.MergeUnmerge))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddMergeUnmergeRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> MergeUnmerge(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/merge-unmerge")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetRequiredService<TimeTablePreferencesMergeUnmergeHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(TimeTablePreferencesEndpoint.ExportTimeTable))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(ExportExcelTimeTableRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(ExportExcelTimeTableRequest.IdSchoolAcadyears), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(ExportExcelTimeTableRequest.IdLevel), In = ParameterLocation.Query, Required = false)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> ExportTimeTable(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-export")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetRequiredService<TimeTablePreferencesExportHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(TimeTablePreferencesEndpoint.TimeTableByUser))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetTimeTableByUserRequest.IdSchoolAcademicYear), In = ParameterLocation.Query, Required = false)]
        [OpenApiParameter(nameof(GetTimeTableByUserRequest.IdSchoolUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> TimeTableByUser(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-by-user")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetRequiredService<TimeTablePreferencesGetByUserHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(TimeTablePreferencesEndpoint.TimetableDashboard))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetDashboardTimeTableRequest.IdAcademicyear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDashboardTimeTableRequest.IdLevel), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDashboardTimeTableRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDashboardTimeTableRequest.IdDepartment), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDashboardTimeTableRequest.IdStreaming), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDashboardTimeTableRequest.IdSubjetc), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDashboardTimeTableRequest.Status), In = ParameterLocation.Query)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> TimetableDashboard(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-dashboard")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetRequiredService<TimeTablePreferencesDashboardHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(TimeTablePreferencesEndpoint.TimetableDetail))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(TimetableDetailRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> TimetableDetail(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "-detail")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetRequiredService<TimeTablePreferencesDetailHandler>();
            return handler.Execute(req, cancellationToken);
        }
    }
}

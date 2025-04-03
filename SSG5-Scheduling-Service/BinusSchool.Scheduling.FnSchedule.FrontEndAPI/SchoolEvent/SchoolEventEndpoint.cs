using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolEvent;
using BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolsEvent;
using BinusSchool.Data.Model.User.FnCommunication.Message;
using BinusSchool.Scheduling.FnSchedule.EventType;
using BinusSchool.Scheduling.FnSchedule.SchoolEvent;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Scheduling.FnSchedule.SchoolsEvent
{
    public class SchoolEventEndpoint
    {
        private const string _route = "schedule/school-event";
        private const string _tag = "School Event";

        [FunctionName(nameof(SchoolEventEndpoint.GetSchoolEvent))]
        [OpenApiOperation(tags: _tag, Summary = "Get School Event List")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetSchoolEventRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSchoolEventRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSchoolEventRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetSchoolEventRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetSchoolEventRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSchoolEventRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSchoolEventRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSchoolEventRequest.StartDate), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSchoolEventRequest.EndDate), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSchoolEventRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSchoolEventRequest.IdEventType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSchoolEventRequest.AssignedAs), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSchoolEventRequest.ApprovalStatus), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetSchoolEventResult))]
        public Task<IActionResult> GetSchoolEvent(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetSchoolEventHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(SchoolEventEndpoint.GetSchoolEventAcademicCalendar))]
        [OpenApiOperation(tags: _tag, Summary = "Get School Event of Academic Calendar List")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetSchoolEventAcademicRequest.IdAcadyear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSchoolEventAcademicRequest.IdLevel), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSchoolEventAcademicRequest.StartDate), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSchoolEventAcademicRequest.EndDate), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetSchoolEventResult))]
        public Task<IActionResult> GetSchoolEventAcademicCalendar(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/academic-calendar")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetSchoolEventAcademicHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(SchoolEventEndpoint.DeleteSchoolEvent))]
        [OpenApiOperation(tags: _tag, Summary = "Delete School Event")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(IEnumerable<string>), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> DeleteSchoolEvent(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route)] HttpRequest req,
            [Queue("notification-em-schedule")] ICollector<string> collector,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<DeleteSchoolEventHandler>();
            return handler.Execute(req, cancellationToken, keyValues: nameof(collector).WithValue(collector));
        }

        [FunctionName(nameof(SchoolEventEndpoint.AddSchoolEvent))]
        [OpenApiOperation(tags: _tag, Summary = "Add School Event")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddSchoolEventRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> AddSchoolEvent(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<AddSchoolEventHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(SchoolEventEndpoint.GetDefaultEventApproverSetting))]
        [OpenApiOperation(tags: _tag, Summary = "Get Default Event Approver Setting")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetDefaultEventApproverSettingRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetDefaultEventApproverSettingResult))]
        public Task<IActionResult> GetDefaultEventApproverSetting(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/default-event-approver-setting")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetDefaultEventApproverSettingHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(SchoolEventEndpoint.GetUserByRolePosition))]
        [OpenApiOperation(tags: _tag, Summary = "Get User By Role Position")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetUserByRolePositionRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetUserByRolePositionRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetUserByRolePositionRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetUserByRolePositionRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetUserByRolePositionRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetUserByRolePositionRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetUserByRolePositionRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetUserByRolePositionRequest.IdRole), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetUserByRolePositionRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetUserByRolePositionRequest.CodePosition), In = ParameterLocation.Query, Required = false)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetUserByRolePositionResult))]
        public Task<IActionResult> GetUserByRolePosition(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-user-by-role-position")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetUserByRolePositionHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(SchoolEventEndpoint.UpdateSchoolEvent))]
        [OpenApiOperation(tags: _tag, Summary = "Update School Event")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateSchoolEventRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> UpdateSchoolEvent(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<UpdateSchoolEventHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(SchoolEventEndpoint.SetDefaultEventApproverSetting))]
        [OpenApiOperation(tags: _tag, Summary = "Set Default Event Approver Setting")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(SetDefaultEventApproverSettingRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> SetDefaultEventApproverSetting(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route + "/set-default-event-approver-setting")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<SetDefaultEventApproverSettingHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(SchoolEventEndpoint.GetSchoolEventApproval))]
        [OpenApiOperation(tags: _tag, Summary = "Get School Event Approval List")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetSchoolEventApprovalRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSchoolEventApprovalRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSchoolEventApprovalRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetSchoolEventApprovalRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetSchoolEventApprovalRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSchoolEventApprovalRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSchoolEventApprovalRequest.StartDate), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSchoolEventApprovalRequest.EndDate), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSchoolEventApprovalRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSchoolEventApprovalRequest.IdStudent), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSchoolEventApprovalRequest.ApprovalStatus), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetSchoolEventApprovalResult))]
        public Task<IActionResult> GetSchoolEventApproval(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-approval")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetSchoolEventApprovalHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(SchoolEventEndpoint.GetStudentParticipant))]
        [OpenApiOperation(tags: _tag, Summary = "Get Student Participant")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetStudentParticipantRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetStudentParticipantRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetStudentParticipantRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetStudentParticipantRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetStudentParticipantRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetStudentParticipantRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetStudentParticipantRequest.IdEvent), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetStudentParticipantRequest.IdActivity), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetStudentParticipantRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetSchoolEventResult))]
        public Task<IActionResult> GetStudentParticipant(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = "schedule/get-student-participant-from-event-setting")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetStudentParticipantHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(SchoolEventEndpoint.SetSchoolEventApprovalStatus))]
        [OpenApiOperation(tags: _tag, Summary = "Set School Event Approval Status")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(SetSchoolEventApprovalStatusRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> SetSchoolEventApprovalStatus(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route + "/set-school-event-approval-status")] HttpRequest req,
            [Queue("notification-em-schedule")] ICollector<string> collector,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<SetSchoolEventApprovalStatusHandler>();
            return handler.Execute(req, cancellationToken, keyValues: nameof(collector).WithValue(collector));
        }

        [FunctionName(nameof(SchoolEventEndpoint.DetailEventSetting))]
        [OpenApiOperation(tags: _tag, Summary = "Get Detail Event Setting")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(DetailEventSettingRequest.Id), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(DetailEventSettingRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(DetailEventSettingRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(DetailEventSettingResult))]
        public Task<IActionResult> DetailEventSetting(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-setting-detail")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<DetailEventSettingDetailHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(SchoolEventEndpoint.DetailRecordOfInvolvement))]
        [OpenApiOperation(tags: _tag, Summary = "Get Detail Record Of Involvement")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(DetailRecordOfInvolvementRequest.IdEvent), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(DetailRecordOfInvolvementRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(DetailRecordOfInvolvementResult))]
        public Task<IActionResult> DetailRecordOfInvolvement(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = "schedule/detail-record-of-involvement")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<DetailRecordOfInvolvementHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(SchoolEventEndpoint.DetailRecordOfInvolvementTeacher))]
        [OpenApiOperation(tags: _tag, Summary = "Get Detail Record Of Involvement")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(DetailRecordOfInvolvementTeacherRequest.IdEvent), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(DetailRecordOfInvolvementTeacherRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(DetailRecordOfInvolvementTeacherResult))]
        public Task<IActionResult> DetailRecordOfInvolvementTeacher(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = "schedule/detail-record-of-involvement-teacher")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<DetailRecordOfInvolvementTeacherHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(SchoolEventEndpoint.GetSchoolEventCalender))]
        [OpenApiOperation(tags: _tag, Summary = "Get Event Calender")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetEventCalendarRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetEventCalendarRequest.StartDate), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetEventCalendarRequest.EndDate), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetEventCalendarRequest.IdEventTypes), In = ParameterLocation.Query, Type = typeof(string[]))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetEventCalendarResult))]
        public Task<IActionResult> GetSchoolEventCalender(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-calender")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetEventCalenderHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(SchoolEventEndpoint.GetSchoolEventInvolvement))]
        [OpenApiOperation(tags: _tag, Summary = "Get Event Involvement List")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetSchoolEventInvolvementRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSchoolEventInvolvementRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSchoolEventInvolvementRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetSchoolEventInvolvementRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetSchoolEventInvolvementRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSchoolEventInvolvementRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSchoolEventInvolvementRequest.StartDate), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSchoolEventInvolvementRequest.EndDate), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSchoolEventInvolvementRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSchoolEventInvolvementRequest.IdStudent), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSchoolEventInvolvementRequest.IdActivity), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSchoolEventInvolvementRequest.IdAward), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetSchoolEventInvolvementResult))]
        public Task<IActionResult> GetSchoolEventInvolvement(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-involvement")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetSchoolEventInvolvementHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(SchoolEventEndpoint.GetSchoolEventInvolvementTeacher))]
        [OpenApiOperation(tags: _tag, Summary = "Get Event Involvement Teacher List")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetSchoolEventInvolvementTeacherRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSchoolEventInvolvementTeacherRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSchoolEventInvolvementTeacherRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetSchoolEventInvolvementTeacherRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetSchoolEventInvolvementTeacherRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSchoolEventInvolvementTeacherRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSchoolEventInvolvementTeacherRequest.StartDate), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSchoolEventInvolvementTeacherRequest.EndDate), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSchoolEventInvolvementTeacherRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSchoolEventInvolvementTeacherRequest.IdActivity), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSchoolEventInvolvementTeacherRequest.IdAward), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetSchoolEventInvolvementTeacherResult))]
        public Task<IActionResult> GetSchoolEventInvolvementTeacher(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-involvement-teacher")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetSchoolEventInvolvementTeacherHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(SchoolEventEndpoint.GetHistoryInvolvementByStudent))]
        [OpenApiOperation(tags: _tag, Summary = "Get History Involvement By Student")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetHistoryInvolvementByStudentRequest.IdStudent), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetHistoryInvolvementByStudentResult))]
        public Task<IActionResult> GetHistoryInvolvementByStudent(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = "schedule/get-history-involvement-by-student")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetSchoolEventInvolvementHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(SchoolEventEndpoint.CreateSchoolEventInvolvement))]
        [OpenApiOperation(tags: _tag, Summary = "Create School Event Involvement")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(CreateSchoolEventInvolvementRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> CreateSchoolEventInvolvement(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "-involvement")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<CreateSchoolEventInvolvementHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(SchoolEventEndpoint.UpdateSchoolEventInvolvement))]
        [OpenApiOperation(tags: _tag, Summary = "Update School Event Involvement")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateSchoolEventInvolvementRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> UpdateSchoolEventInvolvement(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route + "-involvement-update")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<UpdateSchoolEventInvolvementHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(SchoolEventEndpoint.DeleteSchoolEventInvolvement))]
        [OpenApiOperation(tags: _tag, Summary = "Delete School Event Involvement")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(DeleteSchoolEventInvolvementRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> DeleteSchoolEventInvolvement(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route + "-involvement-delete")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<DeleteSchoolEventInvolvementHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(SchoolEventEndpoint.GetListParentStudent))]
        [OpenApiOperation(tags: _tag, Summary = "Get List Parent Student")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetListParentStudentRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListParentStudentRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListParentStudentRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetListParentStudentRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetListParentStudentRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListParentStudentRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListParentStudentRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetListParentStudentRequest.IdLevel), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListParentStudentRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListParentStudentRequest.IdHomeroom), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetListParentStudentResult))]
        public Task<IActionResult> GetListParentStudent(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = "schedule/get-list-parent-student")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetListParentStudentHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(SchoolEventEndpoint.GetDropdownActivity))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetDropdownActivityRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDropdownActivityRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetDropdownActivityResult))]
        public Task<IActionResult> GetDropdownActivity(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = "schedule/get-list-activity")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetDropdownActivityHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(SchoolEventEndpoint.GetDropdownAward))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetDropdownAwardRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDropdownAwardRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetDropdownAwardResult))]
        public Task<IActionResult> GetDropdownAward(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = "schedule/get-list-award")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetDropdownAwardHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(SchoolEventEndpoint.GetSchoolEventSummary))]
        [OpenApiOperation(tags: _tag, Summary = "Get School Event Summary List")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetSchoolEventSummaryRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSchoolEventSummaryRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSchoolEventSummaryRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetSchoolEventSummaryRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetSchoolEventSummaryRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSchoolEventSummaryRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSchoolEventSummaryRequest.StartDate), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSchoolEventSummaryRequest.EndDate), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSchoolEventSummaryRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSchoolEventSummaryRequest.IntendedFor), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSchoolEventSummaryRequest.IdEvent), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSchoolEventSummaryRequest.IdActivity), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSchoolEventSummaryRequest.IdAward), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetSchoolEventSummary2Result))]
        public Task<IActionResult> GetSchoolEventSummary(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-summary")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetSchoolEventSummaryHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(SchoolEventEndpoint.GetStudentbyHomeromeStudent))]
        [OpenApiOperation(tags: _tag, Summary = "Get Student by HomeroomStudent")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetStudentbyHomeromeRequest.IdHomeroom), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetStudentbyHomeromeResult[]))]
        public Task<IActionResult> GetStudentbyHomeromeStudent(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/student-by-homeroom")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetStudentbyHomeromeStudentHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(SchoolEventEndpoint.GetStudentbyGradeSemester))]
        [OpenApiOperation(tags: _tag, Summary = "Get Student by Grade Semester")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetStudentbyGradeSemesterRequest.IdAcademicYear), In = ParameterLocation.Query,Required =true)]
        [OpenApiParameter(nameof(GetStudentbyGradeSemesterRequest.IdLevel), In = ParameterLocation.Query,Required =true)]
        [OpenApiParameter(nameof(GetStudentbyGradeSemesterRequest.IdGrade), In = ParameterLocation.Query,Required =true)]
        [OpenApiParameter(nameof(GetStudentbyGradeSemesterRequest.Semester), In = ParameterLocation.Query,Required =true)]
        [OpenApiParameter(nameof(GetStudentbyGradeSemesterRequest.IdHomeroom), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetStudentbyHomeromeResult[]))]
        public Task<IActionResult> GetStudentbyGradeSemester(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/student-by-grade-semester")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetStudentbyGradeSemesterHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(SchoolEventEndpoint.DownloadEventSummaryExcel))]
        [OpenApiOperation(tags: _tag, Summary = "Download School Event Summary")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(DownloadEventSummaryRequest.StartDate), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(DownloadEventSummaryRequest.EndDate), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(DownloadEventSummaryRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(DownloadEventSummaryRequest.IntendedFor), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(DownloadEventSummaryRequest.IdEvent), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(DownloadEventSummaryRequest.IdActivity), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(DownloadEventSummaryRequest.IdAward), In = ParameterLocation.Query)]
        public Task<IActionResult> DownloadEventSummaryExcel(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-summary/download-excel")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<DownloadEventSummaryExcelHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(SchoolEventEndpoint.GetSchoolEventHistory))]
        [OpenApiOperation(tags: _tag, Summary = "Get School Event History")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetSchoolEventHistoryRequest.IdEvent), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetSchoolEventHistoryResult))]
        public Task<IActionResult> GetSchoolEventHistory(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-history")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetSchoolEventHistoryHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(SchoolEventEndpoint.GetDataActivityForMerit))]
        [OpenApiOperation(tags: _tag, Summary = "Get School Event Summary List")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetDataActivityForMeritRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDataActivityForMeritRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDataActivityForMeritRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetDataActivityForMeritRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetDataActivityForMeritRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDataActivityForMeritRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDataActivityForMeritRequest.IdEvent), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDataActivityForMeritRequest.IdLevel), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDataActivityForMeritRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDataActivityForMeritRequest.IdHomeroom), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDataActivityForMeritRequest.IdClassroom), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDataActivityForMeritRequest.IdActivity), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDataActivityForMeritRequest.IdAward), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetDataActivityForMeritResult))]
        public Task<IActionResult> GetDataActivityForMerit(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = "schedule/get-data-activity-for-merit")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetDataActivityForMeritHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(SchoolEventEndpoint.CreateActivityDataToMerit))]
        [OpenApiOperation(tags: _tag, Summary = "Create Data Activity For Merit")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(CreateActivityDataToMeritRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> CreateActivityDataToMerit(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "schedule/create-activity-to-merit")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<CreateDataActivityForMeritHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(SchoolEventEndpoint.GetDownloadStudentCertification))]
        [OpenApiOperation(tags: _tag, Summary = "Download Student Certification")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetDownloadStudentCertificationRequest.IdStudent), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDownloadStudentCertificationRequest.IdAcadYears), In = ParameterLocation.Query, Type = typeof(string[]), Required = true)]
        // [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetDownloadStudentCertificationResult[]))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetDownloadStudentCertificationResult))]
        public Task<IActionResult> GetDownloadStudentCertification(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = "schedule/get-download-student-certification")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetDownloadStudentCertificationHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(SchoolEventEndpoint.GetDownloadStudentCertificate))]
        [OpenApiOperation(tags: _tag, Summary = "Download Student Certificate")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetDownloadStudentCertificateRequest.IdEventActivityAward), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetDownloadStudentCertificateResult))]
        public Task<IActionResult> GetDownloadStudentCertificate(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = "schedule/get-download-student-certificate")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetDownloadStudentCertificateHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(SchoolEventEndpoint.DetailHistoryEvent))]
        [OpenApiOperation(tags: _tag, Summary = "Get Detail History Event")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(DetailEventSettingRequest.Id), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(DetailEventSettingRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(DetailHistoryEventResult))]
        public Task<IActionResult> DetailHistoryEvent(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = "schedule/detail-history-event")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<DetailHistoryEventHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(SchoolEventEndpoint.AddHistoryEvent))]
        [OpenApiOperation(tags: _tag, Summary = "Add History Event")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddHistoryEventRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> AddHistoryEvent(
           [HttpTrigger(AuthorizationLevel.Function, "post", Route = "schedule/add-history-event")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<AddHistoryEventHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(SchoolEventEndpoint.GetListParentPermission))]
        [OpenApiOperation(tags: _tag, Summary = "Get List Parent Permission")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetListParentPermissionRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListParentPermissionRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListParentPermissionRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetListParentPermissionRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetListParentPermissionRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListParentPermissionRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListParentPermissionRequest.IdEvent), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetListParentPermissionRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListParentPermissionRequest.IdHomeroom), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListParentPermissionRequest.ApprovalStatus), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetListParentPermissionResult))]
        public Task<IActionResult> GetListParentPermission(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = "schedule/get-list-parent-permission")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetListParentPermissionHandler>();
            return handler.Execute(req, cancellationToken);
        }

        #region Event Schedule
        [FunctionName(nameof(SchoolEventEndpoint.GetEventSchedule))]
        [OpenApiOperation(tags: _tag, Summary = "Get Event Schedule")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetEventScheduleRequest.IdAcademicYear), In = ParameterLocation.Query,Required =true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetEventScheduleResult))]
        public Task<IActionResult> GetEventSchedule(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = "schedule/event-schedule")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetEventScheduleHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(SchoolEventEndpoint.UpdateStatusSyncEventSchedule))]
        [OpenApiOperation(tags: _tag, Summary = "Update Status Sync Event Schedule")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateStatusSyncEventScheduleRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> UpdateStatusSyncEventSchedule(
           [HttpTrigger(AuthorizationLevel.Function, "post", Route = "schedule/update-status-event-school")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<UpdateStatusSyncEventScheduleHandler>();
            return handler.Execute(req, cancellationToken);
        }
        #endregion

        [FunctionName(nameof(SchoolEventEndpoint.QueueEvent))]
        [OpenApiOperation(tags: _tag, Summary = "Queue Event")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(QueueEventRequest.IdSchool), In = ParameterLocation.Query)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> QueueEvent(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/queue")] HttpRequest req,
           [Queue("event-queue")] ICollector<string> collector,
           System.Threading.CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<QueueEventHandler>();
            return handler.Execute(req, cancellationToken, keyValues: nameof(collector).WithValue(collector));
        }
    }
}

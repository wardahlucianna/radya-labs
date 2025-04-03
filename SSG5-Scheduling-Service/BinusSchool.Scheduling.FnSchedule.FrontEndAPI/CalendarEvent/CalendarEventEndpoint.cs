//using System;
//using System.Collections.Generic;
//using System.Net;
//using System.Threading;
//using System.Threading.Tasks;
//using BinusSchool.Common.Extensions;
//using BinusSchool.Common.Model;
//using BinusSchool.Data.Model;
//using BinusSchool.Data.Model.Scheduling.FnSchedule.CalendarEvent;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Azure.WebJobs;
//using Microsoft.Azure.WebJobs.Extensions.Http;
//using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
//using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.OpenApi.Models;

//namespace BinusSchool.Scheduling.FnSchedule.CalendarEvent
//{
//    public class CalendarEventEndpoint
//    {
//        private const string _route = "schedule/calendar/event";
//        private const string _tag = "Calendar Event";

//        [FunctionName(nameof(CalendarEventEndpoint.GetListEventType))]
//        [OpenApiOperation(tags: _tag, Summary = "Get Event Type List")]
//        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
//        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
//        [OpenApiParameter(nameof(CollectionRequest.Search), In = ParameterLocation.Query)]
//        [OpenApiParameter(nameof(CollectionRequest.Return), In = ParameterLocation.Query)]
//        [OpenApiParameter(nameof(CollectionRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
//        [OpenApiParameter(nameof(CollectionRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
//        [OpenApiParameter(nameof(CollectionRequest.OrderBy), In = ParameterLocation.Query)]
//        [OpenApiParameter(nameof(CollectionRequest.OrderType), In = ParameterLocation.Query)]
//        [OpenApiParameter(nameof(CollectionRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
//        [OpenApiParameter(nameof(CollectionSchoolRequest.IdSchool), In = ParameterLocation.Query)]
//        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(CalendarEventTypeVm))]
//        public Task<IActionResult> GetListEventType(
//            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/type")] HttpRequest req,
//            CancellationToken cancellationToken)
//        {
//            throw new NotImplementedException();
//        }

//        [FunctionName(nameof(CalendarEventEndpoint.GetEvents))]
//        [OpenApiOperation(tags: _tag, Summary = "Get Event List")]
//        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
//        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
//        [OpenApiParameter(nameof(CollectionRequest.Ids), In = ParameterLocation.Query, Type = typeof(string[]))]
//        [OpenApiParameter(nameof(CollectionRequest.Search), In = ParameterLocation.Query)]
//        [OpenApiParameter(nameof(CollectionRequest.Return), In = ParameterLocation.Query)]
//        [OpenApiParameter(nameof(CollectionRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
//        [OpenApiParameter(nameof(CollectionRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
//        [OpenApiParameter(nameof(CollectionRequest.OrderBy), In = ParameterLocation.Query)]
//        [OpenApiParameter(nameof(CollectionRequest.OrderType), In = ParameterLocation.Query)]
//        [OpenApiParameter(nameof(CollectionRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
//        [OpenApiParameter(nameof(CollectionSchoolRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
//        [OpenApiParameter(nameof(GetCalendarEventRequest.StartDate), In = ParameterLocation.Query, Type = typeof(DateTime), Required = true)]
//        [OpenApiParameter(nameof(GetCalendarEventRequest.EndDate), In = ParameterLocation.Query, Type = typeof(DateTime), Required = true)]
//        [OpenApiParameter(nameof(GetCalendarEventRequest.IdEventType), In = ParameterLocation.Query)]
//        [OpenApiParameter(nameof(GetCalendarEventRequest.IdUser), In = ParameterLocation.Query)]
//        [OpenApiParameter(nameof(GetCalendarEventRequest.IdLevel), In = ParameterLocation.Query)]
//        [OpenApiParameter(nameof(GetCalendarEventRequest.Role), In = ParameterLocation.Query)]
//        [OpenApiParameter(nameof(GetCalendarEventRequest.ExcludeOptionMetadata), In = ParameterLocation.Query, Type = typeof(bool))]
//        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetCalendarEventResult[]))]
//        public Task<IActionResult> GetEvents(
//            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
//            CancellationToken cancellationToken)
//        {
//            throw new NotImplementedException();
//        }

//        [FunctionName(nameof(CalendarEventEndpoint.GetEventDetail))]
//        [OpenApiOperation(tags: _tag, Summary = "Get Event Detail")]
//        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
//        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
//        [OpenApiParameter("id", Required = true)]
//        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetCalendarEventDetailResult))]
//        public Task<IActionResult> GetEventDetail(
//            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/detail/{id}")] HttpRequest req,
//            string id,
//            CancellationToken cancellationToken)
//        {
//            throw new NotImplementedException();
//        }

//        [FunctionName(nameof(CalendarEventEndpoint.AddEvent))]
//        [OpenApiOperation(tags: _tag, Summary = "Add Event", Description = @"
//            - idEventType: GET /scheduling-fn-schedule/schedule/calendar/event/type
//            - idGrade: GET /school-fn-school/school/grade
//            - idDepartment: GET /school-fn-subject/school/department
//            - idSubject: GET /school-fn-subject/school/subject
//            - Role: BinusSchool.Common.Constants.RoleConstant.cs")]
//        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
//        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
//        [OpenApiRequestBody("application/json", typeof(AddCalendarEventRequest))]
//        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
//        public Task<IActionResult> AddEvent(
//            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route)] HttpRequest req,
//            CancellationToken cancellationToken)
//        {
//            throw new NotImplementedException();
//        }

//        [FunctionName(nameof(CalendarEventEndpoint.UpdateEvent))]
//        [OpenApiOperation(tags: _tag, Summary = "Update Event", Description = @"
//            - idEventType: GET /scheduling-fn-schedule/schedule/calendar/event/type
//            - idGrade: GET /school-fn-school/school/grade
//            - idDepartment: GET /school-fn-subject/school/department
//            - idSubject: GET /school-fn-subject/school/subject
//            - Role: BinusSchool.Common.Constants.RoleConstant.cs")]
//        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
//        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
//        [OpenApiRequestBody("application/json", typeof(UpdateCalendarEventRequest))]
//        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
//        public Task<IActionResult> UpdateEvent(
//            [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route)] HttpRequest req,
//            CancellationToken cancellationToken)
//        {
//            throw new NotImplementedException();
//        }

//        [FunctionName(nameof(CalendarEventEndpoint.DeleteEvent))]
//        [OpenApiOperation(tags: _tag, Summary = "Delete Grade")]
//        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
//        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
//        [OpenApiRequestBody("application/json", typeof(IEnumerable<string>), Required = true)]
//        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
//        public Task<IActionResult> DeleteEvent(
//            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route)] HttpRequest req,
//            CancellationToken cancellationToken)
//        {
//            throw new NotImplementedException();
//        }

//        #region Calendar Event v2

//        [FunctionName(nameof(CalendarEventEndpoint.GetEvents2))]
//        [OpenApiOperation(tags: _tag, Summary = "Get Event List")]
//        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
//        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
//        [OpenApiParameter(nameof(CollectionRequest.Ids), In = ParameterLocation.Query, Type = typeof(string[]))]
//        [OpenApiParameter(nameof(CollectionRequest.Search), In = ParameterLocation.Query)]
//        [OpenApiParameter(nameof(CollectionRequest.Return), In = ParameterLocation.Query)]
//        [OpenApiParameter(nameof(CollectionRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
//        [OpenApiParameter(nameof(CollectionRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
//        [OpenApiParameter(nameof(CollectionRequest.OrderBy), In = ParameterLocation.Query)]
//        [OpenApiParameter(nameof(CollectionRequest.OrderType), In = ParameterLocation.Query)]
//        [OpenApiParameter(nameof(CollectionRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
//        [OpenApiParameter(nameof(CollectionSchoolRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
//        [OpenApiParameter(nameof(GetCalendarEvent2Request.StartDate), In = ParameterLocation.Query, Type = typeof(DateTime), Required = true)]
//        [OpenApiParameter(nameof(GetCalendarEvent2Request.EndDate), In = ParameterLocation.Query, Type = typeof(DateTime), Required = true)]
//        [OpenApiParameter(nameof(GetCalendarEvent2Request.IdEventType), In = ParameterLocation.Query)]
//        [OpenApiParameter(nameof(GetCalendarEvent2Request.IdUser), In = ParameterLocation.Query)]
//        [OpenApiParameter(nameof(GetCalendarEvent2Request.IdLevel), In = ParameterLocation.Query)]
//        [OpenApiParameter(nameof(GetCalendarEvent2Request.Role), In = ParameterLocation.Query)]
//        [OpenApiParameter(nameof(GetCalendarEvent2Request.ExcludeHiddenEvent), In = ParameterLocation.Query, Type = typeof(bool))]
//        [OpenApiParameter(nameof(GetCalendarEvent2Request.ExcludeOptionMetadata), In = ParameterLocation.Query, Type = typeof(bool))]
//        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetCalendarEvent2Result[]))]
//        public Task<IActionResult> GetEvents2(
//            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-v2")] HttpRequest req,
//            CancellationToken cancellationToken)
//        {
//            var handler = req.HttpContext.RequestServices.GetService<GetCalendarEvent2Handler>();
//            return handler.Execute(req, cancellationToken);
//        }

//        [FunctionName(nameof(CalendarEventEndpoint.GetEventDetail2))]
//        [OpenApiOperation(tags: _tag, Summary = "Get Event Detail")]
//        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
//        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
//        [OpenApiParameter("id", Required = true)]
//        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetCalendarEvent2DetailResult))]
//        public Task<IActionResult> GetEventDetail2(
//            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-v2/detail/{id}")] HttpRequest req,
//            string id,
//            CancellationToken cancellationToken)
//        {
//            var handler = req.HttpContext.RequestServices.GetService<GetCalendarEvent2DetailHandler>();
//            return handler.Execute(req, cancellationToken, keyValues: "id".WithValue(id));
//        }

//        [FunctionName(nameof(CalendarEventEndpoint.AddEvent2))]
//        [OpenApiOperation(tags: _tag, Summary = "Add Event", Description = @"
//            - idEventType: GET /scheduling-fn-schedule/schedule/calendar/event/type
//            - role: BinusSchool.Common.Constants.RoleConstant.cs (ALL|STAFF|PARENT|TEACHER|STUDENT)
//            - option: BinusSchool.Common.Model.Enums.EventOptionType.cs (None|Grade|Department|Subject|Personal)
//            - forTeacher.idGrades: GET /school-fn-school/school/grade
//            - forTeacher.idDepartments: GET /school-fn-subject/school/department
//            - forTeacher.subjects.idGrade: GET /school-fn-subject/school/subject
//            - forTeacher.subjects.idSubjects: GET /school-fn-subject/school/subject
//            - forStudent.idStudents: GET /scheduling-fn-schedule/schedule/student-enrollment/student
//            - forStudent.idSubjects: GET /scheduling-fn-schedule/schedule/student-enrollment/subject
//            - forStudent.idHomerooms: GET /scheduling-fn-schedule/schedule/student-enrollment/homeroom")]
//        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
//        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
//        [OpenApiRequestBody("application/json", typeof(AddCalendarEvent2Request))]
//        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
//        public Task<IActionResult> AddEvent2(
//            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "-v2")] HttpRequest req,
//            CancellationToken cancellationToken)
//        {
//            var handler = req.HttpContext.RequestServices.GetService<AddCalendarEvent2Handler>();
//            return handler.Execute(req, cancellationToken);
//        }

//        [FunctionName(nameof(CalendarEventEndpoint.UpdateEvent2))]
//        [OpenApiOperation(tags: _tag, Summary = "Update Event", Description = @"
//            See POST method version for reference")]
//        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
//        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
//        [OpenApiRequestBody("application/json", typeof(UpdateCalendarEvent2Request))]
//        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
//        public Task<IActionResult> UpdateEvent2(
//            [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route + "-v2")] HttpRequest req,
//            CancellationToken cancellationToken)
//        {
//            var handler = req.HttpContext.RequestServices.GetService<UpdateCalendarEvent2Handler>();
//            return handler.Execute(req, cancellationToken);
//        }

//        [FunctionName(nameof(CalendarEventEndpoint.DeleteEvent2))]
//        [OpenApiOperation(tags: _tag, Summary = "Delete Grade")]
//        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
//        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
//        [OpenApiRequestBody("application/json", typeof(IEnumerable<string>), Required = true)]
//        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
//        public Task<IActionResult> DeleteEvent2(
//            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route + "-v2")] HttpRequest req,
//            CancellationToken cancellationToken)
//        {
//            var handler = req.HttpContext.RequestServices.GetService<DeleteCalendarEvent2Handler>();
//            return handler.Execute(req, cancellationToken);
//        }

//        #endregion
//    }
//}

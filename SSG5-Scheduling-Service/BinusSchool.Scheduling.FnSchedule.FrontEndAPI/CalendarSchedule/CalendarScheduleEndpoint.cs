using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.CalendarSchedule;
using BinusSchool.Data.Model.School.FnSchool.Grade;
using BinusSchool.Data.Model.School.FnSchool.Level;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;

namespace BinusSchool.Scheduling.FnSchedule.CalendarSchedule
{
    public class CalendarScheduleEndpoint
    {
        private const string _route = "schedule/calendar/schedule";
        private const string _tag = "Calendar Schedule";

        [FunctionName(nameof(CalendarScheduleEndpoint.GetTeacherAcademicYears))]
        [OpenApiOperation(tags: _tag, Summary = "Get Teacher Academic Years")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetTeacherAcademicYearsRequest.Ids), In = ParameterLocation.Query, Type = typeof(string[]))]
        [OpenApiParameter(nameof(GetTeacherAcademicYearsRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetTeacherAcademicYearsRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetTeacherAcademicYearsRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetTeacherAcademicYearsRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetTeacherAcademicYearsRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetTeacherAcademicYearsRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetTeacherAcademicYearsRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetTeacherAcademicYearsRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetTeacherAcademicYearsRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(CodeWithIdVm))]
        public Task<IActionResult> GetTeacherAcademicYears(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/teacher/academic-year")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetTeacherAcademicYearsHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(CalendarScheduleEndpoint.GetTeacherLevels))]
        [OpenApiOperation(tags: _tag, Summary = "Get Teacher Levels")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetTeacherLevelsRequest.Ids), In = ParameterLocation.Query, Type = typeof(string[]))]
        [OpenApiParameter(nameof(GetTeacherLevelsRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetTeacherLevelsRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetTeacherLevelsRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetTeacherLevelsRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetTeacherLevelsRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetTeacherLevelsRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetTeacherLevelsRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetTeacherLevelsRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetTeacherLevelsRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetTeacherLevelsRequest.IdAcadYear), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetLevelResult))]
        public Task<IActionResult> GetTeacherLevels(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/teacher/level")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetTeacherLevelsHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(CalendarScheduleEndpoint.GetTeacherGrades))]
        [OpenApiOperation(tags: _tag, Summary = "Get Teacher Grades")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetTeacherGradesRequest.Ids), In = ParameterLocation.Query, Type = typeof(string[]))]
        [OpenApiParameter(nameof(GetTeacherGradesRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetTeacherGradesRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetTeacherGradesRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetTeacherGradesRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetTeacherGradesRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetTeacherGradesRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetTeacherGradesRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetTeacherGradesRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetTeacherGradesRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetTeacherGradesRequest.IdAcadYear), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetTeacherGradesRequest.IdLevel), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetGradeResult))]
        public Task<IActionResult> GetTeacherGrades(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/teacher/grade")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetTeacherGradesHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(CalendarScheduleEndpoint.GetTeacherSemesters))]
        [OpenApiOperation(tags: _tag, Summary = "Get Teacher Semesters")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetTeacherSemestersRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetTeacherSemestersRequest.IdAcadYear), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetTeacherSemestersRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(int))]
        public Task<IActionResult> GetTeacherSemesters(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/teacher/semester")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetTeacherSemestersHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(CalendarScheduleEndpoint.GetTeacherHomerooms))]
        [OpenApiOperation(tags: _tag, Summary = "Get Teacher Homerooms")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetTeacherHomeroomsRequest.Ids), In = ParameterLocation.Query, Type = typeof(string[]))]
        [OpenApiParameter(nameof(GetTeacherHomeroomsRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetTeacherHomeroomsRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetTeacherHomeroomsRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetTeacherHomeroomsRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetTeacherHomeroomsRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetTeacherHomeroomsRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetTeacherHomeroomsRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetTeacherHomeroomsRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetTeacherHomeroomsRequest.IdAcadYear), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetTeacherHomeroomsRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetTeacherHomeroomsRequest.Semester), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(CodeWithIdVm))]
        public Task<IActionResult> GetTeacherHomerooms(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/teacher/homeroom")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetTeacherHomeroomsHandler>();
            return handler.Execute(req, cancellationToken);
        }


        [FunctionName(nameof(CalendarScheduleEndpoint.GetTeacherHomeroomsV2))]
        [OpenApiOperation(tags: _tag, Summary = "Get Teacher Homerooms V2")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetTeacherHomeroomsRequest.Ids), In = ParameterLocation.Query, Type = typeof(string[]))]
        [OpenApiParameter(nameof(GetTeacherHomeroomsRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetTeacherHomeroomsRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetTeacherHomeroomsRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetTeacherHomeroomsRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetTeacherHomeroomsRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetTeacherHomeroomsRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetTeacherHomeroomsRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetTeacherHomeroomsRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetTeacherHomeroomsRequest.IdAcadYear), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetTeacherHomeroomsRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetTeacherHomeroomsRequest.Semester), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(CodeWithIdVm))]
        public Task<IActionResult> GetTeacherHomeroomsV2(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/teacher/homeroom-v2")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetTeacherHomeroomsV2Handler>();
            return handler.Execute(req, cancellationToken);
        }


        [FunctionName(nameof(CalendarScheduleEndpoint.GetTeachers))]
        [OpenApiOperation(tags: _tag, Summary = "Get Teacher By Homeroom")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetUsersRequest.Ids), In = ParameterLocation.Query, Type = typeof(string[]))]
        [OpenApiParameter(nameof(GetUsersRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetUsersRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetUsersRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetUsersRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetUsersRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetUsersRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetUsersRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetUsersRequest.IdHomeroom), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(ItemValueVm))]
        public Task<IActionResult> GetTeachers(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/teacher")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetTeachersHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(CalendarScheduleEndpoint.GetTeacherSubjects))]
        [OpenApiOperation(tags: _tag, Summary = "Get Teacher Homerooms")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetUserSubjectsRequest.Ids), In = ParameterLocation.Query, Type = typeof(string[]))]
        [OpenApiParameter(nameof(GetUserSubjectsRequest.Position), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetUserSubjectsRequest.IdAcadyear), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetUserSubjectsRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetUserSubjectsRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetUserSubjectsRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetUserSubjectsRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetUserSubjectsRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetUserSubjectsRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetUserSubjectsRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetUserSubjectsRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetUserSubjectsRequest.IdHomeroom), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetUserSubjectsRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(ItemValueVm))]
        public Task<IActionResult> GetTeacherSubjects(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/teacher/subject")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetTeacherSubjectsHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(CalendarScheduleEndpoint.GetListSubjectTeacherPosition))]
        [OpenApiOperation(tags: _tag, Summary = "Get Teacher Homerooms")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetUserSubjectDescriptionRequest.Position), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetUserSubjectDescriptionRequest.IdAcadyear), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetUserSubjectDescriptionRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetUserSubjectDescriptionRequest.IdHomeroom), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetUserSubjectDescriptionRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(ItemValueVm))]
        public Task<IActionResult> GetListSubjectTeacherPosition(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/teacher/list-subject-teacher-position")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetListSubjectTeacherPositionHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(CalendarScheduleEndpoint.GetStudents))]
        [OpenApiOperation(tags: _tag, Summary = "Get Student By Homeroom")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetUsersRequest.Ids), In = ParameterLocation.Query, Type = typeof(string[]))]
        [OpenApiParameter(nameof(GetUsersRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetUsersRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetUsersRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetUsersRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetUsersRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetUsersRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetUsersRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetUsersRequest.IdHomeroom), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(ItemValueVm))]
        public Task<IActionResult> GetStudents(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/student")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetStudentsHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(CalendarScheduleEndpoint.GetStudentSubjects))]
        [OpenApiOperation(tags: _tag, Summary = "Get Student Homerooms")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetUserSubjectsRequest.Ids), In = ParameterLocation.Query, Type = typeof(string[]))]
        [OpenApiParameter(nameof(GetUserSubjectsRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetUserSubjectsRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetUserSubjectsRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetUserSubjectsRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetUserSubjectsRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetUserSubjectsRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetUserSubjectsRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetUserSubjectsRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetUserSubjectsRequest.IdHomeroom), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(ItemValueVm))]
        public Task<IActionResult> GetStudentSubjects(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/student/subject")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetStudentSubjectsHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(CalendarScheduleEndpoint.GetCalendarSchedules))]
        [OpenApiOperation(tags: _tag, Summary = "Get Schedule List", Description = @"
            - Role: BinusSchool.Common.Constants.RoleConstant.cs (STAFF|TEACHER|PARENT|STUDENT)")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(CollectionRequest.Ids), In = ParameterLocation.Query, Type = typeof(string[]))]
        [OpenApiParameter(nameof(CollectionRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(CollectionSchoolRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetCalendarScheduleRequest.StartDate), In = ParameterLocation.Query, Type = typeof(DateTime), Required = true)]
        [OpenApiParameter(nameof(GetCalendarScheduleRequest.EndDate), In = ParameterLocation.Query, Type = typeof(DateTime), Required = true)]
        [OpenApiParameter(nameof(GetCalendarScheduleRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetCalendarScheduleRequest.Role), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetCalendarScheduleRequest.IdHomeroom), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetCalendarScheduleRequest.IdSubject), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetCalendarScheduleRequest.IdAcadyear), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetCalendarScheduleRequest.IdLevel), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetCalendarScheduleRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetCalendarScheduleResult[]))]
        public Task<IActionResult> GetCalendarSchedules(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetCalendarScheduleHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(CalendarScheduleEndpoint.DownloadExcelCalendarSchedule))]
        [OpenApiOperation(tags: _tag, Summary = "Download Schedule List in Excel", Description = @"
            - Role: BinusSchool.Common.Constants.RoleConstant.cs (STAFF|TEACHER|PARENT|STUDENT)")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(DownloadExcelCalendarScheduleRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(DownloadExcelCalendarScheduleRequest.StartDate), In = ParameterLocation.Query, Type = typeof(DateTime), Required = true)]
        [OpenApiParameter(nameof(DownloadExcelCalendarScheduleRequest.EndDate), In = ParameterLocation.Query, Type = typeof(DateTime), Required = true)]
        [OpenApiParameter(nameof(DownloadExcelCalendarScheduleRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(DownloadExcelCalendarScheduleRequest.Role), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(DownloadExcelCalendarScheduleRequest.IdAcadyear), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(DownloadExcelCalendarScheduleRequest.IdLevel), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(DownloadExcelCalendarScheduleRequest.IdGrade), In = ParameterLocation.Query)]
        public Task<IActionResult> DownloadExcelCalendarSchedule(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/download-excel")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<DownloadExcelCalendarScheduleHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(CalendarScheduleEndpoint.DownloadExcelCalendarScheduleV2))]
        [OpenApiOperation(tags: _tag, Summary = "Download Schedule List in Excel V2", Description = @"
            - Role: BinusSchool.Common.Constants.RoleConstant.cs (STAFF|TEACHER|PARENT|STUDENT)")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(DownloadExcelCalendarScheduleRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(DownloadExcelCalendarScheduleRequest.StartDate), In = ParameterLocation.Query, Type = typeof(DateTime), Required = true)]
        [OpenApiParameter(nameof(DownloadExcelCalendarScheduleRequest.EndDate), In = ParameterLocation.Query, Type = typeof(DateTime), Required = true)]
        [OpenApiParameter(nameof(DownloadExcelCalendarScheduleRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(DownloadExcelCalendarScheduleRequest.Role), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(DownloadExcelCalendarScheduleRequest.IdAcadyear), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(DownloadExcelCalendarScheduleRequest.IdLevel), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(DownloadExcelCalendarScheduleRequest.IdGrade), In = ParameterLocation.Query)]
        public Task<IActionResult> DownloadExcelCalendarScheduleV2(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/download-excel-v2")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<DownloadExcelCalendarScheduleV2Handler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(CalendarScheduleEndpoint.GetStudentSubjectsAssignment))]
        [OpenApiOperation(tags: _tag, Summary = "Get Student Subject Assignment")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetUserSubjectsRequest.Ids), In = ParameterLocation.Query, Type = typeof(string[]))]
        [OpenApiParameter(nameof(GetUserSubjectsRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetUserSubjectsRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetUserSubjectsRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetUserSubjectsRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetUserSubjectsRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetUserSubjectsRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetUserSubjectsRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetUserSubjectsRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetUserSubjectsRequest.IdHomeroom), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetUserSubjectsRequest.IdAcadyear), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetUserSubjectsRequest.Position), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(ItemValueVm))]
        public Task<IActionResult> GetStudentSubjectsAssignment(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/student/subject-assignment")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetStudentSubjectsAssignmentHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(CalendarScheduleEndpoint.GetCalendarSchedulesV2))]
        [OpenApiOperation(tags: _tag, Summary = "Get Schedule List V2", Description = @"
            - Role: BinusSchool.Common.Constants.RoleConstant.cs (STAFF|TEACHER|PARENT|STUDENT)")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(CollectionRequest.Ids), In = ParameterLocation.Query, Type = typeof(string[]))]
        [OpenApiParameter(nameof(CollectionRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(CollectionSchoolRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetCalendarScheduleV2Request.StartDate), In = ParameterLocation.Query, Type = typeof(DateTime), Required = true)]
        [OpenApiParameter(nameof(GetCalendarScheduleV2Request.EndDate), In = ParameterLocation.Query, Type = typeof(DateTime), Required = true)]
        [OpenApiParameter(nameof(GetCalendarScheduleV2Request.IdUser), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetCalendarScheduleV2Request.IdUserLogin), In = ParameterLocation.Query, Required=true)]
        [OpenApiParameter(nameof(GetCalendarScheduleV2Request.Role), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetCalendarScheduleV2Request.IdHomeroom), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetCalendarScheduleV2Request.IdSubject), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetCalendarScheduleV2Request.IdAcadyear), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetCalendarScheduleV2Request.IdLevel), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetCalendarScheduleV2Request.IdGrade), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetCalendarScheduleResult[]))]
        public Task<IActionResult> GetCalendarSchedulesV2(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "v2")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetCalendarScheduleV2Handler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(CalendarScheduleEndpoint.GetCalendarScheduleLevel))]
        [OpenApiOperation(tags: _tag, Summary = "Get Calendar Schedule Level")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetCalendarScheduleLevelRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetCalendarScheduleLevelRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetCalendarScheduleLevelResult))]
        public Task<IActionResult> GetCalendarScheduleLevel(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/level")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetCalendarScheduleLevelHandler>();
            return handler.Execute(req, cancellationToken);
        }
    }
}
